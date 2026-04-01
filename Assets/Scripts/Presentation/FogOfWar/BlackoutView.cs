using Core.Events;
using Core.Log;
using Data.Runtime.Events.Map;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Map.Region;
using UnityEngine;

namespace Presentation.FogOfWar
{
    /// <summary>
    /// Renders a full-black mask over locked (unexplored) regions.
    /// Sits between the ground sprite and wall tilemaps in sorting order.
    /// Unlocked regions become transparent, revealing the ground beneath.
    ///
    /// Current behavior: instant snap on unlock (no transition animation).
    /// TODO: Add noise-based dissolve shader effect for unlock animation.
    /// </summary>
    public class BlackoutView : MonoBehaviour
    {
        [Title("Shader")]
        [SerializeField, Required] private Shader blackoutShader;

        [Title("Settings")]
        [OnValueChanged("UpdateMaterial")] [SerializeField] private Color blackoutColor = new(0, 0, 0, 1f);

        [Title("Rendering")]
        [SerializeField] private string sortingLayerName = "Ground";
        [SerializeField] private int sortingOrder = 1;
        [SerializeField] private float padding = 5f;
        [SerializeField, Required] private Grid grid;

        private Texture2D _visibilityTex;
        private Material _material;
        private Vector2Int _mapSize;
        private GameObject _quadObj;
        private bool _initialized;

        private float[] _cellValues;
        private Color[] _pixelBuffer;

        private static readonly int BlackoutColor = Shader.PropertyToID("_BlackoutColor");
        private static readonly int PropVisibilityTex = Shader.PropertyToID("_VisibilityTex");
        private static readonly int PropMapParams     = Shader.PropertyToID("_MapParams");
        private static readonly int PropGridOrigin    = Shader.PropertyToID("_GridOrigin");
        private static readonly int PropInvBasisRow0  = Shader.PropertyToID("_InvBasisRow0");
        private static readonly int PropInvBasisRow1  = Shader.PropertyToID("_InvBasisRow1");

        private IEventBus _eventBus;
        private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

        private IRegionService _regionService;
        private IRegionService RegionService => _regionService ??= LevelContainer.Instance.Resolve<IRegionService>();

        private void OnEnable()
        {
            EventBus.Subscribe<MapViewInitEvent>(OnMapInitialized);
            EventBus.Subscribe<RegionUnlockedEvent>(OnRegionUnlocked);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MapViewInitEvent>(OnMapInitialized);
            EventBus.Unsubscribe<RegionUnlockedEvent>(OnRegionUnlocked);
            Cleanup();
        }

        private void OnMapInitialized(MapViewInitEvent e)
        {
            _mapSize = e.MapData.Size;

            int pixelCount = _mapSize.x * _mapSize.y;
            _cellValues = new float[pixelCount];
            _pixelBuffer = new Color[pixelCount];

            // Read initial region state: unlocked cells = 1 (transparent), locked = 0 (opaque)
            for (int y = 0; y < _mapSize.y; y++)
            {
                for (int x = 0; x < _mapSize.x; x++)
                {
                    int index = y * _mapSize.x + x;
                    _cellValues[index] = RegionService.IsCellUnlocked(new Vector2Int(x, y)) ? 1f : 0f;
                }
            }

            CreateVisibilityTexture();
            UploadTexture();
            CreateMaterial();
            SetupShaderUniforms();
            CreateQuad();
            UpdateMaterial();

            _initialized = true;
            this.Log($"Blackout initialized for {_mapSize.x}x{_mapSize.y} map");
        }

        private void OnRegionUnlocked(RegionUnlockedEvent e)
        {
            if (!_initialized) return;

            // Snap unlocked cells to transparent immediately
            foreach (var cell in e.Cells)
            {
                if (cell.x < 0 || cell.x >= _mapSize.x || cell.y < 0 || cell.y >= _mapSize.y)
                    continue;

                _cellValues[cell.y * _mapSize.x + cell.x] = 1f;
            }

            UploadTexture();
            this.Log($"Blackout: region {e.RegionId} unlocked, {e.Cells.Count} cells revealed");
        }

        private void CreateVisibilityTexture()
        {
            _visibilityTex = new Texture2D(_mapSize.x, _mapSize.y, TextureFormat.R8, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
        }

        private void UploadTexture()
        {
            int count = _cellValues.Length;
            for (int i = 0; i < count; i++)
            {
                float v = _cellValues[i];
                _pixelBuffer[i] = new Color(v, v, v, 1f);
            }
            _visibilityTex.SetPixels(_pixelBuffer);
            _visibilityTex.Apply();
        }

        private void CreateMaterial()
        {
            _material = new Material(blackoutShader);
            _material.SetTexture(PropVisibilityTex, _visibilityTex);
        }

        private void SetupShaderUniforms()
        {
            var center00 = (Vector2)grid.GetCellCenterWorld(Vector3Int.zero);
            var center10 = (Vector2)grid.GetCellCenterWorld(new Vector3Int(1, 0, 0));
            var center01 = (Vector2)grid.GetCellCenterWorld(new Vector3Int(0, 1, 0));
            Vector2 basisX = center10 - center00;
            Vector2 basisY = center01 - center00;

            Vector2 gridOrigin = center00 - 0.5f * basisX - 0.5f * basisY;

            float det = basisX.x * basisY.y - basisX.y * basisY.x;
            if (Mathf.Approximately(det, 0f))
            {
                this.LogError("Grid basis vectors are degenerate (det ≈ 0).");
                return;
            }
            float invDet = 1f / det;
            Vector2 invRow0 = new Vector2(basisY.y, -basisY.x) * invDet;
            Vector2 invRow1 = new Vector2(-basisX.y, basisX.x) * invDet;

            _material.SetVector(PropGridOrigin, new Vector4(gridOrigin.x, gridOrigin.y, 0, 0));
            _material.SetVector(PropInvBasisRow0, new Vector4(invRow0.x, invRow0.y, 0, 0));
            _material.SetVector(PropInvBasisRow1, new Vector4(invRow1.x, invRow1.y, 0, 0));
            _material.SetVector(PropMapParams, new Vector4(
                1f / _mapSize.x, 1f / _mapSize.y,
                _mapSize.x, _mapSize.y));
        }

        private void CreateQuad()
        {
            var center00 = (Vector2)grid.GetCellCenterWorld(Vector3Int.zero);
            var center10 = (Vector2)grid.GetCellCenterWorld(new Vector3Int(1, 0, 0));
            var center01 = (Vector2)grid.GetCellCenterWorld(new Vector3Int(0, 1, 0));
            Vector2 basisX = center10 - center00;
            Vector2 basisY = center01 - center00;
            Vector2 gridOrigin = center00 - 0.5f * basisX - 0.5f * basisY;

            Vector2[] corners =
            {
                gridOrigin,
                gridOrigin + _mapSize.x * basisX,
                gridOrigin + _mapSize.y * basisY,
                gridOrigin + _mapSize.x * basisX + _mapSize.y * basisY
            };

            Vector2 min = corners[0], max = corners[0];
            for (int i = 1; i < 4; i++)
            {
                min = Vector2.Min(min, corners[i]);
                max = Vector2.Max(max, corners[i]);
            }

            min -= Vector2.one * padding;
            max += Vector2.one * padding;

            var mesh = new Mesh
            {
                name = "BlackoutQuad",
                vertices = new Vector3[]
                {
                    new(min.x, min.y, 0),
                    new(max.x, min.y, 0),
                    new(max.x, max.y, 0),
                    new(min.x, max.y, 0)
                },
                triangles = new[] { 0, 2, 1, 0, 3, 2 }
            };
            mesh.RecalculateBounds();

            _quadObj = new GameObject("BlackoutQuad");
            _quadObj.transform.SetParent(transform, false);

            var meshFilter = _quadObj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var meshRenderer = _quadObj.AddComponent<MeshRenderer>();
            meshRenderer.material = _material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.sortingLayerName = sortingLayerName;
            meshRenderer.sortingOrder = sortingOrder;
        }

        private void UpdateMaterial()
        {
	        _material.SetColor(BlackoutColor, blackoutColor);
        }

        private void Cleanup()
        {
            if (_quadObj) Destroy(_quadObj);
            if (_material) Destroy(_material);
            if (_visibilityTex) Destroy(_visibilityTex);

            _quadObj = null;
            _material = null;
            _visibilityTex = null;
            _pixelBuffer = null;
            _cellValues = null;
            _initialized = false;
        }

        #region Debug

        [TitleGroup("Debug")]
        [ShowInInspector, ReadOnly]
        private Vector2Int DebugMapSize => _mapSize;

        [TitleGroup("Debug")]
        [ShowInInspector, ReadOnly]
        private bool DebugInitialized => _initialized;

        #endregion
    }
}

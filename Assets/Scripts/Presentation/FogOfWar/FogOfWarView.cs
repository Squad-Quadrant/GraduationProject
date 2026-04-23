using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Map;
using Data.Runtime.Events.View;
using Data.Runtime.Events.Vision;
using Presentation.Bootstrap;
using Presentation.Unit;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using UnityEngine;

namespace Presentation.FogOfWar
{
	public class FogOfWarView : MonoBehaviour
	{
		[Title("Shader")]
		[SerializeField, Required] private Shader fogShader;
		[PreviewField(ObjectFieldAlignment.Center, Height = 100)]
		[OnValueChanged("UpdateMaterial")] [SerializeField, Required] private Texture2D noiseTex;

		[Title("Fog Settings")]
		[OnValueChanged("UpdateMaterial")] [SerializeField] private Color fogColor = new(0, 0, 0, 0.75f);
		[OnValueChanged("UpdateMaterial")] [SerializeField] private float noiseIntensity = 0.15f;
		[OnValueChanged("UpdateMaterial")] [SerializeField] private float noiseScale = 0.5f;
		[OnValueChanged("UpdateMaterial")] [SerializeField] private float edgeSoftness = 0.35f;
		[OnValueChanged("UpdateMaterial")] [SerializeField] private float clipExtent = 5f;
		[OnValueChanged("OnPaddingParmChange")] [SerializeField, Range(0f, 3.0f)] private int paddingRange = 1;
		[OnValueChanged("OnPaddingParmChange")] [SerializeField, Range(0f, 1.0f)] private float paddingStrength = 0.2f;

		[Title("Animation")]
		[SerializeField, Range(0.01f, 20f)] private float transitionSpeed = 1f;

		[Title("Unit Mask")]
		[OnValueChanged("UpdateMaterial")] [SerializeField] private float unitMaskRadiusX = 0.6f;
		[OnValueChanged("UpdateMaterial")] [SerializeField] private float unitMaskRadiusY = 1.2f;
		[OnValueChanged("UpdateMaterial")] [SerializeField] private float unitCenterOffsetY = 0.4f;
		[OnValueChanged("UpdateMaterial")] [SerializeField, Range(0f, 1f)] private float unitMaskSoftness = 0.4f;

		[Title("Halftone")]
		[OnValueChanged("UpdateMaterial")] [SerializeField, Range(1f, 30f)] private float dotDensity = 8f;
		[OnValueChanged("UpdateMaterial")] [SerializeField, Range(0.3f, 1f)] private float dotMaxRadius = 0.75f;
		[OnValueChanged("UpdateMaterial")] [SerializeField, Range(0f, 1f)] private float dotSoftness = 0.03f;

		[Title("Rendering")]
		[SerializeField] private string sortingLayerName = "OnGround";
		[SerializeField] private int sortingOrder = 1000;
		[SerializeField] private float padding = 5f;

		private Texture2D _visibilityTex;
		private Material _material;
		private Vector2Int _mapSize;
		private GameObject _quadObj;
		private bool _initialized;

		private float[] _targetValues;   // what vision says: 0 or 1
		private float[] _currentValues;  // what's currently displayed: 0~1, lerping toward target
		private Color[] _pixelBuffer;    // reusable buffer for texture upload
		private bool _isDirty;           // true when current != target, triggers Update work

		private const int MaxUnits = 32;
		private readonly Dictionary<string, UnitView> _trackedUnits = new();
		private readonly Vector4[] _unitPositionBuffer = new Vector4[MaxUnits];

		private static readonly int PropVisibilityTex  = Shader.PropertyToID("_VisibilityTex");
		private static readonly int PropNoiseTex       = Shader.PropertyToID("_NoiseTex");
		private static readonly int PropFogColor       = Shader.PropertyToID("_FogColor");
		private static readonly int PropNoiseIntensity = Shader.PropertyToID("_NoiseIntensity");
		private static readonly int PropNoiseScale     = Shader.PropertyToID("_NoiseScale");
		private static readonly int PropEdgeSoftness   = Shader.PropertyToID("_EdgeSoftness");
		private static readonly int PropMapParams      = Shader.PropertyToID("_MapParams");
		private static readonly int PropGridOrigin     = Shader.PropertyToID("_GridOrigin");
		private static readonly int PropInvBasisRow0   = Shader.PropertyToID("_InvBasisRow0");
		private static readonly int PropInvBasisRow1   = Shader.PropertyToID("_InvBasisRow1");
		private static readonly int PropUnitPositions     = Shader.PropertyToID("_UnitPositions");
		private static readonly int PropUnitCount         = Shader.PropertyToID("_UnitCount");
		private static readonly int PropUnitEllipseRadius = Shader.PropertyToID("_UnitEllipseRadius");
		private static readonly int PropUnitCenterOffset  = Shader.PropertyToID("_UnitCenterOffset");
		private static readonly int PropUnitMaskSoftness  = Shader.PropertyToID("_UnitMaskSoftness");
		private static readonly int PropDotDensity   = Shader.PropertyToID("_DotDensity");
		private static readonly int PropDotMaxRadius = Shader.PropertyToID("_DotMaxRadius");
		private static readonly int PropDotSoftness  = Shader.PropertyToID("_DotSoftness");
		private static readonly int PropClipExtent   = Shader.PropertyToID("_ClipExtent");

		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		private ICoordinateConverter _coordinateConverter;
		private ICoordinateConverter CoordinateConverter => _coordinateConverter ??= LevelContainer.Instance.Resolve<ICoordinateConverter>();

		private HashSet<Vector2Int> _visibleCells;

		private void OnEnable()
		{
			EventBus.Subscribe<MapViewInitEvent>(OnMapInitialized);
			EventBus.Subscribe<VisionChangedEvent>(OnVisionChanged);
			EventBus.Subscribe<UnitViewSpawnedEvent>(OnUnitViewSpawned);
			EventBus.Subscribe<UnitViewDespawnedEvent>(OnUnitViewDespawned);
		}

		private void OnDisable()
		{
			if (!RootContainer.Instance) return;
			EventBus.Unsubscribe<MapViewInitEvent>(OnMapInitialized);
			EventBus.Unsubscribe<VisionChangedEvent>(OnVisionChanged);
			EventBus.Unsubscribe<UnitViewSpawnedEvent>(OnUnitViewSpawned);
			EventBus.Unsubscribe<UnitViewDespawnedEvent>(OnUnitViewDespawned);
			Cleanup();
		}

		private void Update()
		{
			if (!_initialized) return;
			UpdateUnitMask();

			if (!_isDirty) return;
			float maxDelta = transitionSpeed * Time.deltaTime;
			bool stillDirty = false;
			int count = _targetValues.Length;

			for (int i = 0; i < count; i++)
			{
				if (Mathf.Approximately(_currentValues[i], _targetValues[i]))
					continue;

				_currentValues[i] = Mathf.MoveTowards(_currentValues[i], _targetValues[i], maxDelta);

				// Check if we still haven't converged
				if (!Mathf.Approximately(_currentValues[i], _targetValues[i]))
					stillDirty = true;
			}

			UploadTexture();
			_isDirty = stillDirty;
		}

		private void OnMapInitialized(MapViewInitEvent e)
		{
			_mapSize = e.MapData.Size;

			int pixelCount = _mapSize.x * _mapSize.y;
			_targetValues = new float[pixelCount];
			_currentValues = new float[pixelCount];
			_pixelBuffer = new Color[pixelCount];

			CreateVisibilityTexture();
			UpdateMaterial();
			SetupShaderUniforms();
			CreateQuad();

			_initialized = true;
			this.Log($"Fog of war initialized for {_mapSize.x}x{_mapSize.y} map");
		}

		private void OnVisionChanged(VisionChangedEvent e)
		{
			_visibleCells = e.VisibleCells;

			for (int y = 0; y < _mapSize.y; y++)
			{
				for (int x = 0; x < _mapSize.x; x++)
				{
					int index = y * _mapSize.x + x;
					_targetValues[index] = _visibleCells.Contains(new Vector2Int(x, y)) ? 1f : 0f;
				}
			}

			if (paddingRange > 0 && paddingStrength is > 0f and < 1f)
				UpdatePadding(_visibleCells);

			_isDirty = true;
		}

		private void OnUnitViewSpawned(UnitViewSpawnedEvent e)
		{
			if (e.View)
				_trackedUnits[e.UnitId] = e.View;
		}

		private void OnUnitViewDespawned(UnitViewDespawnedEvent e)
		{
			_trackedUnits.Remove(e.UnitId);
		}

		private void CreateVisibilityTexture()
		{
			_visibilityTex = new Texture2D(_mapSize.x, _mapSize.y, TextureFormat.R8, false)
			{
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};

			// Start fully fogged
			for (int i = 0; i < _pixelBuffer.Length; i++)
				_pixelBuffer[i] = Color.black;

			_visibilityTex.SetPixels(_pixelBuffer);
			_visibilityTex.Apply();
		}

		private void UpdateMaterial()
		{
			if (!_material) _material = new Material(fogShader);
			_material.SetTexture(PropVisibilityTex, _visibilityTex);
			_material.SetTexture(PropNoiseTex, noiseTex);
			_material.SetColor(PropFogColor, fogColor);
			_material.SetFloat(PropNoiseIntensity, noiseIntensity);
			_material.SetFloat(PropNoiseScale, noiseScale);
			_material.SetFloat(PropEdgeSoftness, edgeSoftness);
			_material.SetVector(PropUnitEllipseRadius, new Vector4(unitMaskRadiusX, unitMaskRadiusY, 0f, 0f));
			_material.SetVector(PropUnitCenterOffset, new Vector4(0f, unitCenterOffsetY, 0f, 0f));
			_material.SetFloat(PropUnitMaskSoftness, unitMaskSoftness);
			_material.SetFloat(PropDotDensity, dotDensity);
			_material.SetFloat(PropDotMaxRadius, dotMaxRadius);
			_material.SetFloat(PropDotSoftness, dotSoftness);
			_material.SetFloat(PropClipExtent, clipExtent);
		}

		private void SetupShaderUniforms()
		{
			var (basisX, basisY) = CoordinateConverter.GetBasis();
			var center00 = CoordinateConverter.GetCenter00();

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
			var (basisX, basisY) = CoordinateConverter.GetBasis();
			var center00 = CoordinateConverter.GetCenter00();

			Vector2 gridOrigin = center00 - 0.5f * basisX - 0.5f * basisY;

			Vector2[] corners =
			{
				gridOrigin,                                              // (0, 0)
				gridOrigin + _mapSize.x * basisX,                        // (mapW, 0)
				gridOrigin + _mapSize.y * basisY,                        // (0, mapH)
				gridOrigin + _mapSize.x * basisX + _mapSize.y * basisY   // (mapW, mapH)
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
				name = "FogQuad",
				vertices = new Vector3[]
				{
					new(min.x, min.y, 0), // bottom-left
					new(max.x, min.y, 0), // bottom-right
					new(max.x, max.y, 0), // top-right
					new(min.x, max.y, 0)  // top-left
				},
				triangles = new[] { 0, 2, 1, 0, 3, 2 }
			};
			mesh.RecalculateBounds();

			_quadObj = new GameObject("FogOfWarQuad");
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

		private void UploadTexture()
		{
			int count = _currentValues.Length;
			for (int i = 0; i < count; i++)
			{
				float v = _currentValues[i];
				_pixelBuffer[i] = new Color(v, v, v, 1f);
			}
			_visibilityTex.SetPixels(_pixelBuffer);
			_visibilityTex.Apply();
		}

		private void UpdatePadding(HashSet<Vector2Int> visibleCells)
		{
			foreach (var cell in visibleCells)
			{
				for (int it = 1; it <= paddingRange; it++)
				{
					float value = paddingStrength * (1f - (float)(it - 1) / paddingRange);
					if (value <= 0) break;

					for (int dir = 0; dir < it; dir++)
					{
						TrySetPadding(cell.x + it - dir, cell.y + dir,      value);  // top-right edge
						TrySetPadding(cell.x - dir,      cell.y + it - dir, value);  // top-left edge
						TrySetPadding(cell.x - it + dir, cell.y - dir,      value);  // bottom-left edge
						TrySetPadding(cell.x + dir,      cell.y - it + dir, value);  // bottom-right edge
					}
				}
			}
		}

		private void TrySetPadding(int x, int y, float value)
		{
			if (x < 0 || x >= _mapSize.x || y < 0 || y >= _mapSize.y) return;

			int index = x + y * _mapSize.x;
			if (_targetValues[index] >= Mathf.Min(1f, value)) return;
			_targetValues[index] = value;
		}

		private void OnPaddingParmChange()
		{
			if (!_initialized) return;

			for (int y = 0; y < _mapSize.y; y++)
			{
				for (int x = 0; x < _mapSize.x; x++)
				{
					int index = y * _mapSize.x + x;
					_targetValues[index] = _visibleCells.Contains(new Vector2Int(x, y)) ? 1f : 0f;
				}
			}

			UpdatePadding(_visibleCells);
			_isDirty = true;
		}

		private void UpdateUnitMask()
		{
			int count = 0;
			foreach (var t in _trackedUnits.Values)
			{
				if (!t || !t.GetVisible()) continue;
				if (count >= MaxUnits) break;

				var pos = t.gameObject.transform.position;
				_unitPositionBuffer[count] = new Vector4(pos.x, pos.y, 0f, 0f);
				count++;
			}

			// Zero out remaining slots so stale data from previous frames
			// doesn't create phantom masks in the shader.
			for (int i = count; i < MaxUnits; i++)
				_unitPositionBuffer[i] = Vector4.zero;

			_material.SetVectorArray(PropUnitPositions, _unitPositionBuffer);
			_material.SetInt(PropUnitCount, count);
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
			_trackedUnits.Clear();
		}

		#region Debug

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly, LabelText("Map Size")]
		private Vector2Int DebugMapSize => _mapSize;

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly, LabelText("Texture Created")]
		private bool DebugTextureCreated => _visibilityTex != null;

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly, LabelText("Quad Created")]
		private bool DebugQuadCreated => _quadObj != null;

		#endregion
	}
}

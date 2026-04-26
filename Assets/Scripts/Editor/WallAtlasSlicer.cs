#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Presentation.Map.Wall;
using Systems.Map;
using Systems.Map.Config;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Editor
{
	public class WallAtlasSlicer : EditorWindow
	{
		[SerializeField] private MapConfig mapConfig;
		[SerializeField] private Grid sceneGrid;
		[SerializeField] private List<Texture2D> leftAtlases = new();
		[SerializeField] private List<Texture2D> rightAtlases = new();
		[SerializeField] private float worldMinX;
		[SerializeField] private float worldMinY;
		[SerializeField] private int overhangTop = 700;
		[SerializeField] private int overhangDown = 50;
		[SerializeField] private int ppu = 400;

		private SerializedObject _serializedSelf;

		[MenuItem("Tools/Slicer/Wall Atlas Slicer")]
		private static void OpenWindow()
		{
			var window = GetWindow<WallAtlasSlicer>("Wall Atlas Slicer");
			window.minSize = new Vector2(450, 300);
		}

		private void OnEnable() => _serializedSelf = new SerializedObject(this);

		private void OnGUI()
		{
			_serializedSelf.Update();

			EditorGUILayout.Space(4);
			EditorGUILayout.LabelField("Wall Atlas Slicer", EditorStyles.boldLabel);
			EditorGUILayout.Space(2);

			mapConfig = (MapConfig)EditorGUILayout.ObjectField(
				"Map Config", mapConfig, typeof(MapConfig), false);
			sceneGrid = (Grid)EditorGUILayout.ObjectField(
				"Scene Grid", sceneGrid, typeof(Grid), true);

			EditorGUILayout.Space(4);

			EditorGUILayout.PropertyField(
				_serializedSelf.FindProperty("leftAtlases"),
				new GUIContent("Left Wall Atlases"), true);
			EditorGUILayout.PropertyField(
				_serializedSelf.FindProperty("rightAtlases"),
				new GUIContent("Right Wall Atlases"), true);

			EditorGUILayout.Space(4);

			EditorGUILayout.BeginHorizontal();
			worldMinX = EditorGUILayout.FloatField("World Min X", worldMinX);
			EditorGUILayout.Space();
			worldMinY = EditorGUILayout.FloatField("World Min Y", worldMinY);
			if (GUILayout.Button("Paste", GUILayout.Width(50)))
				PasteWorldMinFromClipboard();
			EditorGUILayout.EndHorizontal();

			overhangTop = EditorGUILayout.IntSlider("Overhang Top (px)", overhangTop, 0, 1000);
			overhangDown = EditorGUILayout.IntSlider("Overhang Down (px)", overhangDown, 0, 100);
			ppu = EditorGUILayout.IntField("PPU", ppu);

			EditorGUILayout.Space(8);

			GUI.enabled = CanSlice();
			if (GUILayout.Button("Slice & Assign", GUILayout.Height(30)))
				SliceAndAssign();
			GUI.enabled = true;

			if (!CanSlice())
			{
				EditorGUILayout.HelpBox(
					"Assign MapConfig (with wallVisualsPrefab), Scene Grid, " +
					"and at least one atlas texture.",
					MessageType.Info);
			}

			_serializedSelf.ApplyModifiedProperties();
		}

		private bool CanSlice()
		{
			return mapConfig
			       && sceneGrid
			       && (leftAtlases.Count > 0 || rightAtlases.Count > 0)
			       && mapConfig.wallViewPrefab
			       && ppu > 0;
		}

		private void SliceAndAssign()
		{
			var center00 = (Vector2)sceneGrid.GetCellCenterWorld(Vector3Int.zero);
			var center10 = (Vector2)sceneGrid.GetCellCenterWorld(new Vector3Int(1, 0, 0));
			var center01 = (Vector2)sceneGrid.GetCellCenterWorld(new Vector3Int(0, 1, 0));
			Vector2 basisX = center10 - center00;
			Vector2 basisY = center01 - center00;

			var leftWalls = new List<WallSliceData>();
			var rightWalls = new List<WallSliceData>();

			foreach (var wallConfig in mapConfig.walls)
			{
				if (wallConfig == null || wallConfig.WallType == WallType.None) continue;

				var wallKey = wallConfig.WallKey;
				var (pos, isLeft) = wallKey.ToPositionAndIsLeft();

				Vector2 refCenter = center00 + pos.x * basisX + pos.y * basisY;
				Vector2 edgeStart;

				if (isLeft)
					edgeStart = refCenter - 0.5f * basisX + 0.5f * basisY; // Left wall: Left vertex → Top vertex
				else
					edgeStart = refCenter + 0.5f * basisX - 0.5f * basisY; // Right wall: Right vertex → Top vertex

				var edgeEnd = refCenter + 0.5f * basisX + 0.5f * basisY;

				var slice = new WallSliceData
				{
					WallKey = wallKey,
					Position = pos,
					IsLeft = isLeft,
					EdgeStartPx = WorldToPixel(edgeStart),
					EdgeEndPx = WorldToPixel(edgeEnd),
					EdgeMidPx = WorldToPixel((edgeStart + edgeEnd) / 2f),
				};

				if (isLeft) leftWalls.Add(slice);
				else rightWalls.Add(slice);
			}

			var nameToKey = new Dictionary<string, WallKey>();

			ProcessAtlasList(leftAtlases, leftWalls, nameToKey, "L");
			ProcessAtlasList(rightAtlases, rightWalls, nameToKey, "R");

			var keyToSprite = new Dictionary<WallKey, Sprite>();
			foreach (var atlas in leftAtlases) LoadSprites(atlas, nameToKey, keyToSprite);
			foreach (var atlas in rightAtlases) LoadSprites(atlas, nameToKey, keyToSprite);

			Debug.Log($"[WallAtlasSlicer] Loaded {keyToSprite.Count} wall sprites total.");

			AssignSpritesToPrefab(keyToSprite);
		}

		private void ProcessAtlasList(
            List<Texture2D> atlases,
            List<WallSliceData> allWalls,
            Dictionary<string, WallKey> nameToKey,
            string sideLabel)
        {
            if (atlases.Count == 0 || allWalls.Count == 0) return;

            var claimed = new HashSet<WallKey>();
            for (int atlasIdx = 0; atlasIdx < atlases.Count; atlasIdx++)
            {
                var atlas = atlases[atlasIdx];
                if (!atlas) continue;

                string atlasPath = AssetDatabase.GetAssetPath(atlas);
                var (wasReadable, wasMaxSize) = EnsureImportSettings(atlas);

                atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
                atlases[atlasIdx] = atlas;

                var slicesForThisAtlas = new List<WallSliceData>();
                foreach (var wall in allWalls
	                         .Where(wall => !claimed.Contains(wall.WallKey))
	                         .Where(wall => HasContentAtMidpoint(atlas, wall.EdgeMidPx)))
                {
	                slicesForThisAtlas.Add(wall);
	                claimed.Add(wall.WallKey);
                }

                if (slicesForThisAtlas.Count == 0)
                {
                    Debug.Log($"[WallAtlasSlicer] Atlas '{atlas.name}' has no detectable {sideLabel} walls.");
                    continue;
                }

                ApplySliceMetadata(atlas, slicesForThisAtlas, nameToKey, atlasIdx);

                RestoreImportSettings(atlas, wasReadable, wasMaxSize);
                Debug.Log($"[WallAtlasSlicer] Atlas '{atlas.name}': detected {slicesForThisAtlas.Count} {sideLabel} walls.");
            }

            // Warn about unclaimed walls (missing art).
            int missing = allWalls.Count - claimed.Count;
            if (missing <= 0) return;

            // Collect a few examples for the warning message.
            var examples = new List<string>();
            foreach (var wall in allWalls.Where(wall => !claimed.Contains(wall.WallKey)))
            {
	            examples.Add(wall.WallKey.ToString());
	            if (examples.Count >= 3) break;
            }
            Debug.LogWarning($"[WallAtlasSlicer] {missing} {sideLabel} wall(s) not found " +
                             $"in any atlas. Examples: {string.Join(", ", examples)}");
        }

        private static bool HasContentAtMidpoint(Texture2D atlas, Vector2Int midPx)
        {
            const int sampleRadius = 2; // 5×5 area

            int texW = atlas.width;
            int texH = atlas.height;

            for (int dy = -sampleRadius; dy <= sampleRadius; dy++)
            {
                for (int dx = -sampleRadius; dx <= sampleRadius; dx++)
                {
                    int x = midPx.x + dx;
                    int y = midPx.y + dy;

                    if (x < 0 || x >= texW || y < 0 || y >= texH) continue;

                    if (atlas.GetPixel(x, y).a > 0.01f)
                        return true;
                }
            }

            return false;
        }

        private static (bool wasReadable, int wasMaxSize) EnsureImportSettings(Texture2D texture)
        {
	        string path = AssetDatabase.GetAssetPath(texture);
	        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
	        if (!importer) return (true, 8192);

	        bool wasReadable = importer.isReadable;
	        int wasMaxSize = importer.maxTextureSize;
	        bool needReimport = false;

	        if (!wasReadable)   { importer.isReadable = true;       needReimport = true; }
	        if (wasMaxSize < 16384) { importer.maxTextureSize = 16384; needReimport = true; }

	        if (needReimport) importer.SaveAndReimport();
	        return (wasReadable, wasMaxSize);
        }

        private static void RestoreImportSettings(Texture2D texture, bool wasReadable, int wasMaxSize)
        {
	        string path = AssetDatabase.GetAssetPath(texture);
	        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
	        if (!importer) return;

	        bool needReimport = false;
	        if (importer.isReadable != wasReadable)       { importer.isReadable = wasReadable;       needReimport = true; }
	        if (importer.maxTextureSize != wasMaxSize) { importer.maxTextureSize = wasMaxSize; needReimport = true; }

	        if (needReimport) importer.SaveAndReimport();
        }

        private void ApplySliceMetadata(
            Texture2D atlas,
            List<WallSliceData> slices,
            Dictionary<string, WallKey> nameToKey,
            int atlasIndex)
        {
            string atlasPath = AssetDatabase.GetAssetPath(atlas);
            var importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
            if (!importer)
            {
                Debug.LogError($"[WallAtlasSlicer] Cannot get TextureImporter for '{atlasPath}'.");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();

            int texW = atlas.width;
            int texH = atlas.height;
            var spriteRects = new List<SpriteRect>();

            foreach (var slice in slices)
            {
                int minPx = Mathf.Min(slice.EdgeStartPx.x, slice.EdgeEndPx.x);
                int minPy = Mathf.Min(slice.EdgeStartPx.y, slice.EdgeEndPx.y) - overhangDown;
                int maxPx = Mathf.Max(slice.EdgeStartPx.x, slice.EdgeEndPx.x);
                int maxPy = Mathf.Max(slice.EdgeStartPx.y, slice.EdgeEndPx.y) + overhangTop;

                minPx = Mathf.Max(0, minPx);
                minPy = Mathf.Max(0, minPy);
                maxPx = Mathf.Min(texW, maxPx);
                maxPy = Mathf.Min(texH, maxPy);

                int rectW = maxPx - minPx;
                int rectH = maxPy - minPy;
                if (rectW <= 0 || rectH <= 0)
                {
                    Debug.LogWarning($"[WallAtlasSlicer] Degenerate rect for {slice.WallKey}, skipping.");
                    continue;
                }

                float pivotX = Mathf.Clamp01((float)(slice.EdgeMidPx.x - minPx) / rectW);
                float pivotY = Mathf.Clamp01((float)(slice.EdgeMidPx.y - minPy) / rectH);

                string side = slice.IsLeft ? "L" : "R";
                string spriteName = $"wall_{slice.Position.x}_{slice.Position.y}_{side}_a{atlasIndex}";
                spriteRects.Add(new SpriteRect
                {
                    name = spriteName,
                    rect = new Rect(minPx, minPy, rectW, rectH),
                    pivot = new Vector2(pivotX, pivotY),
                    alignment = SpriteAlignment.Custom,
                    spriteID = GUID.Generate(),
                });

                nameToKey[spriteName] = slice.WallKey;
            }

            // Write sprite rects through the data provider, then apply + reimport.
            dataProvider.SetSpriteRects(spriteRects.ToArray());
            dataProvider.Apply();

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static void LoadSprites(
            Texture2D atlas,
            Dictionary<string, WallKey> nameToKey,
            Dictionary<WallKey, Sprite> keyToSprite)
        {
            if (!atlas) return;

            string atlasPath = AssetDatabase.GetAssetPath(atlas);
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);

            foreach (var asset in allAssets)
            {
                if (asset is not Sprite sprite) continue;
                if (!nameToKey.TryGetValue(sprite.name, out var key)) continue;
                keyToSprite[key] = sprite;
            }
        }

        private void AssignSpritesToPrefab(Dictionary<WallKey, Sprite> keyToSprite)
        {
            var prefab = mapConfig.wallViewPrefab;
            string prefabPath = AssetDatabase.GetAssetPath(prefab);

            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("[WallAtlasSlicer] Cannot find prefab asset path.");
                return;
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            var views = prefabRoot.GetComponentsInChildren<WallView>();

            int assigned = 0;
            int missing = 0;

            foreach (var view in views)
            {
                var key = view.WallKey;
                if (keyToSprite.TryGetValue(key, out var sprite))
                {
                    view.Renderer.sprite = sprite;
                    assigned++;
                }
                else
                {
                    Debug.LogWarning($"[WallAtlasSlicer] No sprite for {key} on '{view.gameObject.name}'.");
                    missing++;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            Debug.Log($"[WallAtlasSlicer] Assigned {assigned} sprites" + (missing > 0 ? $" ({missing} missing)." : "."));
        }

        private Vector2Int WorldToPixel(Vector2 worldPos)
        {
            int px = Mathf.RoundToInt((worldPos.x - worldMinX) * ppu) + 1;
            int py = Mathf.RoundToInt((worldPos.y - worldMinY) * ppu) + 1;
            return new Vector2Int(px, py);
        }

        private void PasteWorldMinFromClipboard()
        {
            string clip = GUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(clip)) return;

            string[] parts = clip.Split(',');
            if (parts.Length == 2
                && float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float x)
                && float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float y))
            {
                worldMinX = x;
                worldMinY = y;
                Debug.Log($"[WallAtlasSlicer] Pasted worldMin: ({x:F6}, {y:F6})");
                Repaint();
            }
            else
            {
                Debug.LogWarning($"[WallAtlasSlicer] Clipboard '{clip}' is not in 'x,y' format.");
            }
        }

		private struct WallSliceData
		{
			public WallKey WallKey;
			public Vector2Int Position;  // Canonical position from ToPositionAndIsLeft
			public bool IsLeft;
			public Vector2Int EdgeStartPx; // Edge start in pixel coordinates
			public Vector2Int EdgeEndPx;   // Edge end in pixel coordinates
			public Vector2Int EdgeMidPx;   // Edge midpoint in pixel coordinates (for pivot)
		}
	}
}
#endif

#if UNITY_EDITOR
using System.Collections.Generic;
using Systems.Map.Config.SceneActor;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Editor
{
	public class SceneActorAtlasSlicer : EditorWindow
	{
		[SerializeField] private AtlasSliceManifest manifest;

		private SerializedObject _serializedSelf;

		[MenuItem("Tools/Slicer/SceneActor Atlas Slicer")]
		private static void OpenWindow()
		{
			var window = GetWindow<SceneActorAtlasSlicer>("SceneActor Slicer");
			window.minSize = new Vector2(160, 160);
		}

		private void OnEnable() => _serializedSelf = new SerializedObject(this);

		private void OnGUI()
		{
			_serializedSelf.Update();

			EditorGUILayout.Space(4);
			EditorGUILayout.LabelField("SceneActor Atlas Slicer", EditorStyles.boldLabel);
			EditorGUILayout.Space(4);

			EditorGUILayout.PropertyField(_serializedSelf.FindProperty(nameof(manifest)));

			EditorGUILayout.Space(8);

			GUI.enabled = CanSlice();
			if (GUILayout.Button("Slice", GUILayout.Height(30)))
				Slice();
			GUI.enabled = true;

			if (!CanSlice())
				EditorGUILayout.HelpBox("Assign a manifest with valid atlas, sizes, and at least one actor.", MessageType.Info);

			_serializedSelf.ApplyModifiedProperties();
		}

		private bool CanSlice()
		{
			if (!manifest || !manifest.atlas) return false;
			if (manifest.atlasGridSize.x <= 0 || manifest.atlasGridSize.y <= 0) return false;
			if (manifest.gameCellSize.x <= 0 || manifest.gameCellSize.y <= 0) return false;
			return manifest.configs is { Count: > 0 };
		}

		private void Slice()
		{
			int atlasGridW = manifest.atlasGridSize.x;
			int atlasGridH = manifest.atlasGridSize.y;
			int gameCellW = manifest.gameCellSize.x;
			int gameCellH = manifest.gameCellSize.y;

			Vector2 basisXPx = new(gameCellW * 0.5f, gameCellH * 0.5f);
			Vector2 basisYPx = new(-gameCellW * 0.5f, gameCellH * 0.5f);

			string atlasPath = AssetDatabase.GetAssetPath(manifest.atlas);
			var (wasReadable, wasMaxSize) = EnsureImportSettings(manifest.atlas);
			var importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
			if (!importer)
			{
				Debug.LogError($"[SceneActorSlicer] Cannot get TextureImporter for '{atlasPath}'.");
				return;
			}

			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Multiple;

			var factory = new SpriteDataProviderFactories();
			factory.Init();
			var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
			dataProvider.InitSpriteEditorDataProvider();

			int texW = manifest.atlas.width;
			int texH = manifest.atlas.height;

			var spriteRects = new List<SpriteRect>();
			// nameToTarget: sprite name → (Config, cellOffset)；切片完成后用此查表回填
			var nameToTarget = new Dictionary<string, (SceneActorConfig config, Vector2Int offset)>();

			foreach (var config in manifest.configs)
			{
				if (!config)
				{
					Debug.LogWarning("[SceneActorSlicer] Skipping entry with null config.");
					continue;
				}

				Vector2 baseCenterPx = new(
					config.atlasOriginCell.x * atlasGridW + atlasGridW * 0.5f,
					config.atlasOriginCell.y * atlasGridH + gameCellH * 0.5f
				);

				// cellOffset 集合 = (0,0) ∪ extraGrid
				var offsets = new List<Vector2Int> { Vector2Int.zero };
				offsets.AddRange(config.extraGrid);

				var cellCenters = new Vector2[offsets.Count];
				for (int i = 0; i < offsets.Count; i++)
					cellCenters[i] = baseCenterPx + offsets[i].x * basisXPx + offsets[i].y * basisYPx;

				for (int i = 0; i < offsets.Count; i++)
				{
					var (rect, pivot) = ComputeCellRect(cellCenters[i], cellCenters, i, atlasGridW, atlasGridH, gameCellH, texW, texH);

					if (rect.width <= 0 || rect.height <= 0)
					{
						Debug.LogWarning($"[SceneActorSlicer] Degenerate rect for '{config.name}' offset {offsets[i]}, skipping.");
						continue;
					}

					string spriteName = $"{config.name}_{offsets[i].x}_{offsets[i].y}";
					spriteRects.Add(new SpriteRect
					{
						name = spriteName,
						rect = rect,
						pivot = pivot,
						alignment = SpriteAlignment.Custom,
						spriteID = GUID.Generate(),
					});
					nameToTarget[spriteName] = (config, offsets[i]);
				}
			}

			// 全量覆盖：上次切的 SpriteRect 作废
			dataProvider.SetSpriteRects(spriteRects.ToArray());
			dataProvider.Apply();
			EditorUtility.SetDirty(importer);
			importer.SaveAndReimport();
			RestoreImportSettings(manifest.atlas, wasReadable, wasMaxSize);

			Debug.Log($"[SceneActorSlicer] Sliced {spriteRects.Count} sub-sprites in '{manifest.atlas.name}'.");

			// 回填 baseSlices
			WriteBackBaseSlices(atlasPath, nameToTarget);
		}

		private static (Rect rect, Vector2 pivot) ComputeCellRect(
			Vector2 thisCenter, Vector2[] allCenters, int thisIndex,
			int atlasGridW, int atlasGridH, int gameCellH,
			int texW, int texH)
		{
			float minX = thisCenter.x - atlasGridW * 0.5f;
			float maxX = thisCenter.x + atlasGridW * 0.5f;
			float minY = thisCenter.y - gameCellH * 0.5f;
			float maxY = thisCenter.y - gameCellH * 0.5f + atlasGridH;

			for (int i = 0; i < allCenters.Length; i++)
			{
				if (i == thisIndex) continue;

				var other = allCenters[i];
				float dx = other.x - thisCenter.x;
				float dy = other.y - thisCenter.y;
				float midX = (thisCenter.x + other.x) * 0.5f;
				float midY = (thisCenter.y + other.y) * 0.5f;

				if (Mathf.Abs(dx) >= Mathf.Abs(dy))
				{
					if (dx > 0)
						maxX = Mathf.Min(maxX, midX);
					else
						minX = Mathf.Max(minX, midX);
				}
				else
				{
					if (dy > 0)
						maxY = Mathf.Min(maxY, midY);
					else
						minY = Mathf.Max(minY, midY);
				}
			}

			int rectMinX = Mathf.Max(0, Mathf.RoundToInt(minX));
			int rectMinY = Mathf.Max(0, Mathf.RoundToInt(minY));
			int rectMaxX = Mathf.Min(texW, Mathf.RoundToInt(maxX));
			int rectMaxY = Mathf.Min(texH, Mathf.RoundToInt(maxY));

			int rectW = rectMaxX - rectMinX;
			int rectH = rectMaxY - rectMinY;
			if (rectW <= 0 || rectH <= 0) return (default, default);

			float pivotX = (thisCenter.x - rectMinX) / rectW;
			float pivotY = (thisCenter.y - rectMinY) / rectH;

			return (new Rect(rectMinX, rectMinY, rectW, rectH), new Vector2(pivotX, pivotY));
		}

		private static void WriteBackBaseSlices(string atlasPath, Dictionary<string, (SceneActorConfig config, Vector2Int offset)> nameToTarget)
		{
			var allAssets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);

			// 首次遇到某 Config 时清空其 baseSlices，再 Add；防止旧切片残留
			var clearedConfigs = new HashSet<SceneActorConfig>();

			foreach (var asset in allAssets)
			{
				if (asset is not Sprite sprite) continue;
				if (!nameToTarget.TryGetValue(sprite.name, out var target)) continue;

				if (clearedConfigs.Add(target.config))
					target.config.baseSlices.Clear();

				target.config.baseSlices.Add(new SpriteSlice
				{
					cellOffset = target.offset,
					sprite = sprite,
				});
			}

			foreach (var config in clearedConfigs)
				EditorUtility.SetDirty(config);
			AssetDatabase.SaveAssets();

			Debug.Log($"[SceneActorSlicer] Wrote baseSlices to {clearedConfigs.Count} configs.");
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
	}
}
#endif

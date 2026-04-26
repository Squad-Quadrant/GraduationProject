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
			window.minSize = new Vector2(380, 160);
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

				var padding = manifest.padding ?? new RectOffset();

				Vector2 baseCenterPx = new(
					config.atlasOriginCell.x * atlasGridW + atlasGridW * 0.5f,
					config.atlasOriginCell.y * atlasGridH + gameCellH * 0.5f
				);

				// cellOffset 集合 = (0,0) ∪ extraGrid
				var offsets = new List<Vector2Int> { Vector2Int.zero };
				offsets.AddRange(config.extraGrid);

				foreach (var offset in offsets)
				{
					Vector2 cellCenterPx = baseCenterPx + offset.x * basisXPx + offset.y * basisYPx;

					// rect = cell 中心 ± gameCellSize/2 + padding 外扩，clamp 到 texture 边界
					int rectMinX = Mathf.Max(0, Mathf.RoundToInt(cellCenterPx.x - gameCellW * 0.5f - padding.left));
					int rectMinY = Mathf.Max(0, Mathf.RoundToInt(cellCenterPx.y - gameCellH * 0.5f - padding.bottom));
					int rectMaxX = Mathf.Min(texW, Mathf.RoundToInt(cellCenterPx.x + gameCellW * 0.5f + padding.right));
					int rectMaxY = Mathf.Min(texH, Mathf.RoundToInt(cellCenterPx.y + gameCellH * 0.5f + padding.top));

					int rectW = rectMaxX - rectMinX;
					int rectH = rectMaxY - rectMinY;
					if (rectW <= 0 || rectH <= 0)
					{
						Debug.LogWarning($"[SceneActorSlicer] Degenerate rect for '{config.name}' offset {offset}, skipping.");
						continue;
					}

					// pivot：cell 中心在 rect 内的归一化坐标
					float pivotX = (cellCenterPx.x - rectMinX) / rectW;
					float pivotY = (cellCenterPx.y - rectMinY) / rectH;

					string spriteName = $"{config.name}_{offset.x}_{offset.y}";
					spriteRects.Add(new SpriteRect
					{
						name = spriteName,
						rect = new Rect(rectMinX, rectMinY, rectW, rectH),
						pivot = new Vector2(pivotX, pivotY),
						alignment = SpriteAlignment.Custom,
						spriteID = GUID.Generate(),
					});
					nameToTarget[spriteName] = (config, offset);
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

using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Map;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Map;
using Systems.Map.Config;
using Systems.Map.Region;
using UnityEditor;
using UnityEngine;

namespace Presentation.Map.Wall
{
	public class WallViewManager : MonoBehaviour
	{
		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		private IMapService _mapService;
		private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();

		private IRegionService _regionService;
		private IRegionService RegionService => _regionService ??= LevelContainer.Instance.Resolve<IRegionService>();

		private Dictionary<WallKey, WallView> _wallLookup;
		private Vector2Int? _previousHoverCellPos;

		private void OnEnable()
		{
			EventBus.Subscribe<PointerHoverEvent>(OnPointerHover);
			EventBus.Subscribe<MapCellStateChangedEvent>(OnMapCellChanged);
			EventBus.Subscribe<RegionUnlockedEvent>(OnRegionUnlocked);
		}

		private void OnDisable()
		{
			EventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);
			EventBus.Unsubscribe<MapCellStateChangedEvent>(OnMapCellChanged);
			EventBus.Unsubscribe<RegionUnlockedEvent>(OnRegionUnlocked);

			_wallLookup = null;
		}

		internal void Initialize(MapData mapData)
		{
			// collect wall views
			_wallLookup = new Dictionary<WallKey, WallView>();

			var visuals = GetComponentsInChildren<WallView>();
			foreach (var visual in visuals)
			{
				var key = visual.WallKey;
				if (!_wallLookup.TryAdd(key, visual))
					this.LogWarning($"Duplicate WallVisual for {key} on '{visual.gameObject.name}', skipping.");
			}
			this.Log($"Collected {_wallLookup.Count} wall visuals.");

			// validate
			foreach (var wall in mapData.Walls.Values)
			{
				if (wall == null || wall.Type == WallType.None) continue;

				if (!_wallLookup.ContainsKey(wall.Key))
					this.LogWarning($"Logical wall {wall.Key} (Type={wall.Type}) has no WallVisual in scene.");
			}

			foreach (var wall in mapData.Walls.Values)
			{
				if (wall == null) continue;
				SetWallAlpha(wall.Key, IsWallRegionVisible(wall.Key) ? 1f : 0f);
			}
		}

		private void OnPointerHover(PointerHoverEvent e)
		{
			if (!e.CellPosition.HasValue) return;

			if (_previousHoverCellPos.HasValue)
			{
				var previousWalls = MapService.GetWallsWhichHideCell(_previousHoverCellPos.Value);
				foreach (var wall in previousWalls
					         .Where(wall => wall != null)
					         .Where(wall => IsWallRegionVisible(wall.Key)))
					SetWallAlpha(wall.Key, MapService.CheckWallTransparency(wall) ? 0.5f : 1f);
			}

			var walls = MapService.GetWallsWhichHideCell(e.CellPosition.Value);
			foreach (var wall in walls.Where(wall => wall != null))
				SetWallAlpha(wall.Key, MapService.CheckWallTransparency(wall) ? 0.5f : 1f);

			_previousHoverCellPos = e.CellPosition;
		}

		private void OnMapCellChanged(MapCellStateChangedEvent e)
		{
			foreach (var wall in e.Walls)
			{
				if (wall == null) continue;
				SetWallAlpha(wall.Key, MapService.CheckWallTransparency(wall) ? 0.5f : 1f);
			}
		}

		private void OnRegionUnlocked(RegionUnlockedEvent e)
		{
			foreach (var wallKey in e.BoundaryWalls)
			{
				var wall = MapService.Data.GetWall(wallKey);
				if (wall == null) continue;
				SetWallAlpha(wallKey, IsWallRegionVisible(wallKey) ? 1f : 0f);
			}

			foreach (var cellPos in e.Cells)
			{
				var cell = MapService.Data.GetCell(cellPos);
				if (cell == null) continue;

				var neighbors = new Vector2Int[]
				{
					new(cellPos.x + 1, cellPos.y),
					new(cellPos.x - 1, cellPos.y),
					new(cellPos.x, cellPos.y + 1),
					new(cellPos.x, cellPos.y - 1)
				};

				foreach (var neighbor in neighbors)
				{
					var wallKey = new WallKey(cellPos, neighbor);
					var wall = MapService.Data.GetWall(wallKey);
					if (wall == null) continue;
					SetWallAlpha(wallKey, IsWallRegionVisible(wallKey) ? 1f : 0f);
				}
			}
		}

		private void SetWallAlpha(WallKey key, float alpha)
		{
			if (_wallLookup == null) return;
			if (!_wallLookup.TryGetValue(key, out var visual)) return;

			var c = visual.Renderer.color;
			visual.Renderer.color = new Color(c.r, c.g, c.b, alpha);
		}

		private bool IsWallRegionVisible(WallKey wallKey)
		{
			var (cellA, isLeft) = wallKey.ToPositionAndIsLeft();

			var cellB = isLeft
				? new Vector2Int(cellA.x, cellA.y + 1)
				: new Vector2Int(cellA.x + 1, cellA.y);

			return RegionService.IsCellUnlocked(cellA) || RegionService.IsCellUnlocked(cellB);
		}

#if UNITY_EDITOR
        [TitleGroup("Editor Tools")]
        [SerializeField, Required, LabelText("Map Config")]
        private MapConfig editorMapConfig;

        [TitleGroup("Editor Tools")]
        [SerializeField, Required, LabelText("Scene Grid")]
        private Grid editorSceneGrid;

        [TitleGroup("Editor Tools")]
        [SerializeField, LabelText("Prefab Save Path")]
        [FolderPath(ParentFolder = "Assets")]
        private string editorPrefabFolder = "Res/Prefabs/Map";

        [TitleGroup("Editor Tools")]
        [Button("Generate Wall Objects", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
        private void EditorGenerateWallObjects()
        {
            if (!editorMapConfig)
            {
                Debug.LogError("[WallView] MapConfig is not assigned.");
                return;
            }

            if (!editorSceneGrid)
            {
                Debug.LogError("[WallView] Scene Grid is not assigned.");
                return;
            }

            Undo.RecordObject(this, "Generate Wall Objects");
            EditorClearWallVisualChildren();

            int created = 0;
            foreach (var wallConfig in editorMapConfig.walls)
            {
	            if (wallConfig == null || wallConfig.WallType == WallType.None) continue;

	            var wallKey = wallConfig.WallKey;
	            var (pos, isLeft) = wallKey.ToPositionAndIsLeft();

	            // Edge midpoint = average of the two adjacent cell centers.
	            var cellAWorld = editorSceneGrid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
	            var cellBWorld = editorSceneGrid.GetCellCenterWorld(isLeft
		            ? new Vector3Int(pos.x, pos.y + 1, 0)
		            : new Vector3Int(pos.x + 1, pos.y, 0));
	            Vector3 wallWorldPos = (cellAWorld + cellBWorld) / 2f;

	            string side = isLeft ? "L" : "R";
	            var go = new GameObject($"Wall_{pos.x}_{pos.y}_{side}");
	            go.transform.SetParent(transform, false);
	            go.transform.position = wallWorldPos;

	            // RequireComponent on WallVisual auto-adds SpriteRenderer.
	            var visual = go.AddComponent<WallView>();
	            visual.Setup(wallConfig.position1, wallConfig.position2);

	            Undo.RegisterCreatedObjectUndo(go, "Create WallVisual");
	            created++;
            }

            Debug.Log($"[WallView] Generated {created} wall objects.");
            EditorUtility.SetDirty(this);
        }

        [TitleGroup("Editor Tools")]
        [Button("Save As Prefab & Assign", ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.6f)]
        private void EditorSaveAsPrefab()
        {
	        if (!editorMapConfig)
	        {
		        Debug.LogError("[WallView] MapConfig is not assigned.");
		        return;
	        }

	        var visuals = GetComponentsInChildren<WallView>();
	        if (visuals.Length == 0)
	        {
		        Debug.LogWarning("[WallView] No WallVisual children found. Run 'Generate Wall Objects' first.");
		        return;
	        }

	        string folderPath = $"Assets/{editorPrefabFolder}";
	        EnsureFolderExists(folderPath);

	        // Save this GO directly — WallView is the prefab root.
	        string prefabPath = $"{folderPath}/{editorMapConfig.MapName}_Walls.prefab";
	        var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath, out bool success);

	        if (success)
	        {
		        Undo.RecordObject(editorMapConfig, "Assign Wall Visuals Prefab");
		        editorMapConfig.wallViewPrefab = prefab;
		        EditorUtility.SetDirty(editorMapConfig);
		        AssetDatabase.SaveAssets();

		        Debug.Log($"[WallView] Saved prefab to '{prefabPath}' and assigned to MapConfig.");
	        }
	        else
	        {
		        Debug.LogError($"[WallView] Failed to save prefab to '{prefabPath}'.");
	        }
        }

        [TitleGroup("Editor Tools")]
        [Button("Clear Wall Objects", ButtonSizes.Medium), GUIColor(1f, 0.6f, 0.6f)]
        private void EditorClearWallVisualChildren()
        {
	        var children = (from Transform child in transform where child.GetComponent<WallView>() select child.gameObject).ToList();

	        foreach (var child in children)
		        Undo.DestroyObjectImmediate(child);

	        if (children.Count > 0)
		        Debug.Log($"[WallView] Cleared {children.Count} wall objects.");
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            string[] parts = folderPath.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
#endif
	}
}

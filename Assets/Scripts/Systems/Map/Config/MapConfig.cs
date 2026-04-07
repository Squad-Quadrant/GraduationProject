using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Map.Config
{
	[CreateAssetMenu(fileName = "NewMapConfig", menuName = "Game/Map Config")]
	public class MapConfig : ScriptableObject
	{
#if UNITY_EDITOR
		[LabelText("地图名称")]
		public string editedMapName = "New Map";
		
		[LabelText("地图尺寸")]
		[MinValue(5)]
		public Vector2Int editedSize = new(10, 10);

		private bool Dirty => editedMapName != mapName || editedSize != size;

		[ShowIf("Dirty")]
		[Button("保存更改", ButtonSizes.Medium), GUIColor(0.6f, 1f, 0.6f)]
		private void Confirm()
		{
			if (editedMapName != mapName) OnMapNameChanged();
			if (editedSize != size) OnSizeChanged();
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssets();
		}
		
		[ShowIf("Dirty")]
		[Button("还原更改", ButtonSizes.Medium), GUIColor(1f, 0.6f, 0.6f)]
		private void Revert()
		{
			editedMapName = mapName;
			editedSize = size;
		}
#endif
		[SerializeField][HideInInspector]private string mapName = "New Map";
		public string MapName => mapName;
		
		[SerializeField][HideInInspector]private Vector2Int size = new(10, 10);
		public Vector2Int Size => size;


		[Title("地面与区域")]
		[PropertyOrder(0)]
		[LabelText("地面整图")]
		[PreviewField(ObjectFieldAlignment.Center, Height = 64)]
		public Sprite groundSprite;

		[PropertyOrder(0)]
		[LabelText("墙体预制体")]
		public GameObject wallViewPrefab;

		[PropertyOrder(0)]
		[LabelText("区域定义")]
		[TableList(AlwaysExpanded = true)]
		public RegionDefinition[] regions = { RegionDefinition.DefaultOutdoor };


		[LabelText("地形配置")]
		[PropertyOrder(1)]
		[TableList(ShowIndexLabels = true)]
		public CellConfigData[] cells = Array.Empty<CellConfigData>();

		[LabelText("墙体配置")]
        [PropertyOrder(2)]
        [TableList(ShowIndexLabels = true)]
        public WallConfigData[] walls = Array.Empty<WallConfigData>();

        public int CellCount => Size.x * Size.y;
        public int WallCount => (Size.x - 1) * Size.y + (Size.y - 1) * Size.x;

		#region Tools

		[Button("验证配置", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
		public void ValidateConfig()
		{
			Debug.Log($"[MapConfig] Validating '{mapName}'...");

			var positions = new HashSet<Vector2Int>();
			foreach (var cell in cells)
				if (!positions.Add(cell.position))
					Debug.LogError($"Duplicate position found: {cell.position}");

			// Validate region references
			var regionIds = new HashSet<int>();
			foreach (var region in regions)
				if (!regionIds.Add(region.regionId))
					Debug.LogError($"Duplicate region ID: {region.regionId}");

			foreach (var cell in cells)
				if (!regionIds.Contains(cell.regionId))
					Debug.LogWarning($"Cell {cell.position} references undefined region {cell.regionId}");

			Debug.Log($"[MapConfig] Validation complete. Total cells: {cells.Length}");
		}

		[ShowInInspector, DisplayAsString, HideLabel]
		[PropertyOrder(-1)]
		private string EditorTitle => $"地图配置: {mapName}";

		#endregion

		public void Init()
        {
	        if (regions == null || regions.Length == 0)
		        regions = new[] { RegionDefinition.DefaultOutdoor };

            cells = new CellConfigData[Size.x * Size.y];
            int index = 0;
            for (int y = 0; y < Size.y; y++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    cells[index] = new CellConfigData
                    {
                        position = new Vector2Int(x, y),
                        regionId = 0
                    };
                    index++;
                }
            }

            walls = new WallConfigData[WallCount];
            int wallIndex = 0;
            for (int y = 0; y < Size.y; y++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    if (x < Size.x - 1)
                    {
                        walls[wallIndex++] = new WallConfigData
                        {
                            position1 = new Vector2Int(x, y),
                            position2 = new Vector2Int(x + 1, y),
                        };
                    }
                    if (y < Size.y - 1)
                    {
                        walls[wallIndex++] = new WallConfigData
                        {
                            position1 = new Vector2Int(x, y),
                            position2 = new Vector2Int(x, y + 1),
                        };
                    }
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        private void OnSizeChanged()
        {
            size = editedSize;
            var temp = cells;
            cells = new CellConfigData[Size.x * Size.y];
            int index = 0;
            for (int y = 0; y < Size.y; y++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    var pos = new Vector2Int(x, y);
                    var existingCell = Array.Find(temp, c => c.position == pos);
                    if (existingCell != null)
                    {
                        cells[index] = existingCell;
                    }
                    else
                    {
                        cells[index] = new CellConfigData
                        {
                            position = pos,
                            regionId = 0
                        };
                    }
                    index++;
                }
            }
        
            // 处理 walls 的迁移与重建
            var tempWalls = walls ?? Array.Empty<WallConfigData>();
            walls = new WallConfigData[WallCount];
            int wallIndex = 0;
            for (int y = 0; y < Size.y; y++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    // 水平墙（x -> x+1）
                    if (x < Size.x - 1)
                    {
                        var p1 = new Vector2Int(x, y);
                        var p2 = new Vector2Int(x + 1, y);
                        var existingWall = Array.Find(tempWalls, w => w != null && w.Check(p1, p2));
                        if (existingWall != null)
                        {
                            walls[wallIndex++] = existingWall;
                        }
                        else
                        {
                            walls[wallIndex++] = new WallConfigData
                            {
                                position1 = p1,
                                position2 = p2,
                            };
                        }
                    }
        
                    // 垂直墙（y -> y+1）
                    if (y < Size.y - 1)
                    {
                        var p1 = new Vector2Int(x, y);
                        var p2 = new Vector2Int(x, y + 1);
                        var existingWall = Array.Find(tempWalls, w => w != null && w.Check(p1, p2));
                        if (existingWall != null)
                        {
                            walls[wallIndex++] = existingWall;
                        }
                        else
                        {
                            walls[wallIndex++] = new WallConfigData
                            {
                                position1 = p1,
                                position2 = p2,
                            };
                        }
                    }
                }
            }
        }


		private void OnMapNameChanged()
		{
#if UNITY_EDITOR
			mapName = editedMapName;
			string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
			UnityEditor.AssetDatabase.RenameAsset(assetPath, mapName);
#endif
		}

	}

	/// <summary>
	/// Defines a single cell in the map grid.
	/// </summary>
	[Serializable]
	public class CellConfigData
	{
		[HorizontalGroup("Main")]
		[LabelText("坐标"), LabelWidth(20), ReadOnly]
		public Vector2Int position;

        public ETerrainType Terrain => !cell ? ETerrainType.Void : cell.terrainType;

        public bool IsWalkable => cell && cell.isWalkable;

        public int MoveCost => !cell ? int.MaxValue : cell.moveCost;

        [HorizontalGroup("Props")] 
        [LabelText("地块"), LabelWidth(40)]
        [CanBeNull]
        public CellConfig cell;

        [HorizontalGroup("Props")] 
        [LabelText("场景物体"), LabelWidth(40)]
        [CanBeNull]
        public SceneActorConfig sceneActor;

        [HorizontalGroup("Props")]
        [LabelText("区域"), LabelWidth(30)]
        public int regionId;
    }

    [Serializable]
    public class WallConfigData
    {
        public Vector2Int position1;

        public Vector2Int position2;

        public WallKey WallKey => new(position1, position2);

        public WallConfig wall;

        public WallType WallType => !wall ? WallType.None : wall.wallType;

        public bool Check(Vector2Int posA, Vector2Int posB)
	        => (position1 == posA && position2 == posB) || (position1 == posB && position2 == posA);
    }
}

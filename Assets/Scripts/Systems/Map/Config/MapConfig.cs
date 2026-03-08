using System;
using System.Collections.Generic;
using Data.Config;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Map.Config
{
	[CreateAssetMenu(fileName = "NewMapConfig", menuName = "Game/Map Config")]
	public class MapConfig : ScriptableObject
	{
		#region Basic Info
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
			OnMapNameChanged();
			OnSizeChanged();
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

		#endregion
		
		#region Terrain Data

		[LabelText("地形配置")]
		[PropertyOrder(1)]
		[TableList(ShowIndexLabels = true)]
		public CellConfigData[] cells = Array.Empty<CellConfigData>();
        public int CellCount => Size.x * Size.y;
        [LabelText("墙体配置")]
        [PropertyOrder(2)]
        [TableList(ShowIndexLabels = true)]
        public WallConfigData[] walls = Array.Empty<WallConfigData>();
        public int WallCount => (Size.x - 1) * Size.y + (Size.y - 1) * Size.x;
		#endregion

		#region Tools

		[Button("验证配置", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
		public void ValidateConfig()
		{
			Debug.Log($"[MapConfig] Validating '{mapName}'...");

			var positions = new HashSet<Vector2Int>();
			foreach (var cell in cells)
				if (!positions.Add(cell.position))
					Debug.LogError($"Duplicate position found: {cell.position}");

			Debug.Log($"[MapConfig] Validation complete. Total cells: {cells.Length}");
		}

		#endregion
		
		#region Editor Display

		[ShowInInspector, DisplayAsString, HideLabel]
		[PropertyOrder(-1)]
		private string EditorTitle => $"地图配置: {mapName}";

		#endregion

        public void Init()
        {
            cells = new CellConfigData[Size.x * Size.y];
            int index = 0;
            for (int y = 0; y < Size.y; y++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    cells[index] = new CellConfigData
                    {
                        position = new Vector2Int(x, y),
                        // terrain = ETerrainType.Plain,
                        // IsWalkable = true,
                        // moveCost = 1
                    };
                    index++;
                }
            }

            // 考虑到功能制作的便利性，也初始化墙体数据
            walls = new WallConfigData[WallCount];
            int wallIndex = 0;
            for (int y = 0; y < Size.y; y++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    // 水平墙
                    if (x < Size.x - 1)
                    {
                        walls[wallIndex++] = new WallConfigData
                        {
                            position1 = new Vector2Int(x, y),
                            position2 = new Vector2Int(x + 1, y),
                            // wallType = WallType.None
                        };
                    }
                    // 垂直墙
                    if (y < Size.y - 1)
                    {
                        walls[wallIndex++] = new WallConfigData
                        {
                            position1 = new Vector2Int(x, y),
                            position2 = new Vector2Int(x, y + 1),
                            // wallType = WallType.None
                        };
                    }
                }
            }
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
                            // terrain = ETerrainType.Plain,
                            // IsWalkable = true,
                            // moveCost = 1
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
                                // wallType = WallType.None
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
                                // wallType = WallType.None
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
			UnityEditor.AssetDatabase.SaveAssets();
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

        public ETerrainType Terrain
        {
            get
            {
                if (!cell)
                {
                    return ETerrainType.Void;
                }
                return cell.TerrainType;
            }
        }

        // 指地块本身是否支持通行，不算场景物体等其他因素的影响
        public bool IsWalkable
        {
            get
            {
                if (!cell)
                {
                    return false;
                }
                return cell.IsWalkable;
            }
        }

        public int MoveCost
        {
            get 
            {
                if (!cell)
                {
                    return int.MaxValue;
                }
                return cell.MoveCost;
            }
        }

		// [HorizontalGroup("Props")]
		// [LabelText("高度"), LabelWidth(40)]
		// [Range(0, 5)]
		// public int height = 0;

        [HorizontalGroup("Props")] 
        [LabelText("地块"), LabelWidth(40)]
        [CanBeNull]
        public CellConfig cell;

        [HorizontalGroup("Props")] 
        [LabelText("场景物体"), LabelWidth(40)]
        [CanBeNull]
        public SceneActorConfig sceneActor;
    }

    [Serializable]
    public class WallConfigData
    {
        public Vector2Int position1;
        public Vector2Int position2;
        public WallKey WallKey => new WallKey(position1, position2);
        public WallConfig wall;

        public WallType WallType
        {

            get{
                if (!wall)
                {
                    return WallType.None;
                }
                return wall.wallType;
            }
        }
        
        public bool Check(Vector2Int posA, Vector2Int posB)
        {
            return (position1 == posA && position2 == posB) || (position1 == posB && position2 == posA);
        }
    }
}

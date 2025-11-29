using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Systems.Map;
using UnityEngine;

namespace Data.Config
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

		private bool Dirty => editedMapName != _mapName || editedSize != _size;

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
			editedMapName = _mapName;
			editedSize = _size;
		}
#endif
		private string _mapName = "New Map";
		public string MapName => _mapName;
		
		private Vector2Int _size = new(10, 10);
		public Vector2Int Size => _size;

		#endregion
		
		#region Terrain Data

		[LabelText("地形配置")]
		[PropertyOrder]
		[TableList(ShowIndexLabels = true)]
		public CellConfig[] cells = Array.Empty<CellConfig>();

		#endregion

		#region Tools

		[Button("验证配置", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
		public void ValidateConfig()
		{
			Debug.Log($"[MapConfig] Validating '{_mapName}'...");

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
		private string EditorTitle => $"地图配置: {_mapName}";

		#endregion
		
		public void Init()
		{
			cells = new CellConfig[Size.x * Size.y];
			int index = 0;
			for (int y = 0; y < Size.y; y++)
			{
				for (int x = 0; x < Size.x; x++)
				{
					cells[index] = new CellConfig
					{
						position = new Vector2Int(x, y),
						terrain = ETerrainType.Plain,
						isWalkable = true,
						moveCost = 1
					};
					index++;
				}
			}
		}
		
		private void OnSizeChanged()
		{
			_size = editedSize;
			var temp = cells;
			cells = new CellConfig[Size.x * Size.y];
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
						cells[index] = new CellConfig
						{
							position = pos,
							terrain = ETerrainType.Plain,
							isWalkable = true,
							moveCost = 1
						};
					}
					index++;
				}
			}
		}
		
		private void OnMapNameChanged()
		{
#if UNITY_EDITOR
			_mapName = editedMapName;
			string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
			UnityEditor.AssetDatabase.RenameAsset(assetPath, _mapName);
			UnityEditor.AssetDatabase.SaveAssets();
#endif
		}
	}

	/// <summary>
	/// Defines a single cell in the map grid.
	/// </summary>
	[Serializable]
	public class CellConfig
	{
		[HorizontalGroup("Main")]
		[LabelText("坐标"), LabelWidth(40), ReadOnly]
		public Vector2Int position;

		[HorizontalGroup("Main")]
		[LabelText("地形类型"), LabelWidth(60)]
		public ETerrainType terrain;

		[HorizontalGroup("Props")]
		[LabelText("可通行"), LabelWidth(60)]
		public bool isWalkable = true;

		[HorizontalGroup("Props")]
		[LabelText("移动消耗"), LabelWidth(60)]
		[Range(1, 10)]
		public int moveCost = 1;

		[HorizontalGroup("Props")]
		[LabelText("高度"), LabelWidth(40)]
		[Range(0, 5)]
		public int height = 0;
	}
}

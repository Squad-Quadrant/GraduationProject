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
		[Title("Basic Info")]
		[LabelText("地图名称")]
		public string mapName = "New Map";

		[LabelText("地图尺寸")]
		[MinValue(5)]
		public Vector2Int size = new(10, 10);

		[Title("Terrain Data")]
		[LabelText("地形配置")]
		[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
		public CellConfig[] cells = Array.Empty<CellConfig>();

		[Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
		public void GenerateDefaultTerrain()
		{
			cells = new CellConfig[size.x * size.y];
			int index = 0;
			for (int y = 0; y < size.y; y++)
			{
				for (int x = 0; x < size.x; x++)
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

		[Button(ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
		public void ValidateConfig()
		{
			Debug.Log($"[MapConfig] Validating '{mapName}'...");

			var positions = new HashSet<Vector2Int>();
			foreach (var cell in cells)
				if (!positions.Add(cell.position))
					Debug.LogError($"Duplicate position found: {cell.position}");

			Debug.Log($"[MapConfig] Validation complete. Total cells: {cells.Length}");
		}
	}

	[Serializable]
	public class CellConfig
	{
		[HorizontalGroup("Main")]
		[LabelText("坐标"), LabelWidth(40)]
		public Vector2Int position;

		[HorizontalGroup("Main")]
		[LabelText("地形"), LabelWidth(40)]
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

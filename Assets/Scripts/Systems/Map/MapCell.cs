using JetBrains.Annotations;
using Systems.Map.SceneActor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Map
{
	/// <summary>
	/// Runtime representation of a single cell in the map grid.
	/// </summary>
	public class MapCell
	{
		public Vector2Int Position { get; }
		public ETerrainType Terrain { get; set; }
		public bool IsWalkable { get; set; } = true;
		public int MoveCost { get; set; } = 1;

        public bool IsOccupied => !string.IsNullOrEmpty(UnitId) || SceneActor != null;
        public SceneActorBase SceneActor { get; set; }

        public string UnitId { get; set; }

		public MapCell(Vector2Int position) => Position = position;
	}
}

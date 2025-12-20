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
		// public int Height { get; set; } = 0;
		public bool IsWalkable { get; set; } = true;
		public int MoveCost { get; set; } = 1;
        
        [CanBeNull] public TileBase tile { get; set; } // todo: 放这有点怪 

        public bool IsOccupied
        {
            get
            {
                if (SceneActor != null)
                {
                    return true;
                }
                return false;
            }
        } // Indicates if an entity is currently occupying the cell
        
		// public string OccupantId { get; set; }
        public SceneActorBase SceneActor { get; set; }

		public MapCell(Vector2Int position) => Position = position;
	}
}

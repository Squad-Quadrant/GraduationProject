using PurpleFlowerCore;
using Systems.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Config
{
    [Configurable("Map/Cell")]
    [CreateAssetMenu(fileName = "CellConfig", menuName = "Game/Map/CellConfig")]
    public class CellConfig : ScriptableObject
    {
        public TileBase Tile;
        public bool IsWalkable = true;
        public int MoveCost = 1;
        public ETerrainType TerrainType = ETerrainType.Plain;
    }
}
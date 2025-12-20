using PurpleFlowerCore;
using Systems.Map;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Config
{
    [Configurable("Map/Tile")]
    [CreateAssetMenu(fileName = "TileConfig", menuName = "Configs/Map/TileConfig")]
    public class TileConfig : ScriptableObject
    {
        public TileBase Tile;
        public bool IsWalkable = true;
        public int MoveCost = 1;
        public ETerrainType TerrainType = ETerrainType.Plain;
    }
}
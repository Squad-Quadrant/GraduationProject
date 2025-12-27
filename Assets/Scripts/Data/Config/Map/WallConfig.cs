using PurpleFlowerCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Config.Map
{
    [Configurable("Map/Wall")]
    [CreateAssetMenu(fileName = "WallConfig", menuName = "Game/Map/WallConfig", order = 1)]
    public class WallConfig : ScriptableObject
    {
        public WallType wallType;
        public TileBase leftTile;
        public TileBase rightTile;
    }
    
    public enum WallType
    {
        None,
        LowWall,
        HighWall
    }
}
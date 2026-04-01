using PurpleFlowerCore;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Map.Config
{
    [Configurable("Map/Cell")]
    [CreateAssetMenu(fileName = "CellConfig", menuName = "Game/Map/CellConfig")]
    public class CellConfig : ScriptableObject
    {
        public bool isWalkable = true;
        public int moveCost = 1;
        public ETerrainType terrainType = ETerrainType.Plain;
    }
}

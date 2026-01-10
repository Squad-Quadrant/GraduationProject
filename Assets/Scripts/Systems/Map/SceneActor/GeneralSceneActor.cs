using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
    public class GeneralSceneActor : SceneActorBase
    {
        public GeneralSceneActor(SceneActorType type, uint uid, Tile tile, MapCell baseCell, List<MapCell> extraCells) : base(type, uid)
        {
            Tile = tile;
            BaseCell = baseCell;
            ExtraCells = extraCells;
        }
    }
}
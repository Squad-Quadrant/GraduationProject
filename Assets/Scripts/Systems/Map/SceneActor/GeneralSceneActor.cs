using System.Collections.Generic;
using Systems.Map.Config;
using Systems.Map.Config.SceneActor;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
    public class GeneralSceneActor : SceneActorBase
    {
	    public GeneralSceneActor(uint uid, SceneActorConfig config, MapCell baseCell, IReadOnlyList<MapCell> extraCells)
		    : base(uid, config, baseCell, extraCells) { }
    }
}

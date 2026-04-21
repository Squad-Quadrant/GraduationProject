using System.Collections.Generic;
using Sirenix.OdinInspector;
using Systems.Map.SceneActor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Map.Config
{
    [CreateAssetMenu(fileName = "SceneActorConfig", menuName = "Game/SceneActorConfig")]
    public class SceneActorConfig : ScriptableObject
    {
	    public string displayName;

	    public SceneActorType type;

	    public List<Vector2Int> extraGrid = new();

        public List<Tile> tiles = new();

        public bool blockVision = true;

        public List<Vector2Int> blockMovementFrom = new()
        {
	        new Vector2Int(0, -1),
	        new Vector2Int(0, 1),
	        new Vector2Int(1, 0),
	        new Vector2Int(-1, 0)
        };

        [ShowIf("type", SceneActorType.Door)]
        public int regionId = -1;
    }
}

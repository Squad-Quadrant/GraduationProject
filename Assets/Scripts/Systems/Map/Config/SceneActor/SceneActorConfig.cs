using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Map.Config.SceneActor
{
	[Serializable]
	public class SpriteSlice
	{
		public Vector2Int cellOffset;

		public Sprite sprite;
	}

    [CreateAssetMenu(fileName = "SceneActorConfig", menuName = "Game/Map/SceneActor/General")]
    public class SceneActorConfig : ScriptableObject
    {
	    [Title("Basic")]
	    public string displayName;

	    public List<Vector2Int> extraGrid = new();

        public bool blockVision = true;

        public List<Vector2Int> blockMovementFrom = new()
        {
	        new Vector2Int(0, -1),
	        new Vector2Int(0, 1),
	        new Vector2Int(1, 0),
	        new Vector2Int(-1, 0)
        };

        [Title("Atlas")]
        public Vector2Int atlasOriginCell;

        public List<SpriteSlice> baseSlices = new();
    }
}

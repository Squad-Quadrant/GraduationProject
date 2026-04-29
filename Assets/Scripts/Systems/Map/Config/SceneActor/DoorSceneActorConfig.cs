using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Map.Config.SceneActor
{
	[CreateAssetMenu(fileName = "DoorSceneActorConfig", menuName = "Game/Map/SceneActor/Door")]
	public class DoorSceneActorConfig : SceneActorConfig
	{
		[Title("门")]
		public int regionId = -1;

		public List<SpriteSlice> openedSlices = new();
	}
}

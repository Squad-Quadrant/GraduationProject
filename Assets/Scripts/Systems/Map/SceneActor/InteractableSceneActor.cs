using System.Collections.Generic;
using Core.Events;
using Presentation.Bootstrap;
using Systems.Map.Config;
using Systems.Map.Config.SceneActor;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
	public abstract class InteractableSceneActor : GeneralSceneActor
	{
		protected InteractableSceneActor(uint uid, SceneActorConfig config, MapCell baseCell, IReadOnlyList<MapCell> extraCells)
			: base(uid, config, baseCell, extraCells) { }

		public abstract bool CanInteract { get; }

		public abstract void Interact();
	}
}

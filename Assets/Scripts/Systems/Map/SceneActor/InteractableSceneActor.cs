using System.Collections.Generic;
using Core.Events;
using Presentation.Bootstrap;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
	public abstract class InteractableSceneActor : GeneralSceneActor
	{
		protected InteractableSceneActor(SceneActorType type, uint uid, Tile tile, MapCell baseCell, List<MapCell> extraCells)
			: base(type, uid, tile, baseCell, extraCells) { }

		public abstract bool CanInteract { get; set; }

		public abstract void Interact();
	}
}

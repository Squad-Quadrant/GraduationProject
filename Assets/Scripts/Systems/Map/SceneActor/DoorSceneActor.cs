using System.Collections.Generic;
using Data.Runtime.Events.View;
using Presentation.Bootstrap;
using Systems.Map.Region;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
	public class DoorSceneActor : InteractableSceneActor
	{
		public DoorSceneActor(SceneActorType type, uint uid, Tile tile, MapCell baseCell, List<MapCell> extraCells, int regionId)
			: base(type, uid, tile, baseCell, extraCells)
		{
			_regionId = regionId;
		}

		private readonly int _regionId;

		private IRegionService _regionService;
		private IRegionService RegionService => _regionService ??= LevelContainer.Instance.Resolve<IRegionService>();

		public override bool CanInteract { get; set; } = true;

		public override void Interact()
		{
			if (!CanInteract) return;
			if (RegionService.IsRegionUnlocked(_regionId)) return;

			CanInteract = false;
			RegionService.UnlockRegion(_regionId);
			EventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.Interact));
		}
	}
}

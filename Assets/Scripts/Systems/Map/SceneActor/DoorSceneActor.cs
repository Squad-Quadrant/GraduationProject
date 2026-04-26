using System.Collections.Generic;
using Core.Events;
using Data.Runtime.Events.Map;
using Data.Runtime.Events.View;
using Presentation.Bootstrap;
using Systems.Map.Config;
using Systems.Map.Config.SceneActor;
using Systems.Map.Region;
using UnityEngine.Tilemaps;

namespace Systems.Map.SceneActor
{
	public class DoorSceneActor : InteractableSceneActor
	{
		private readonly DoorSceneActorConfig _doorConfig;
		private bool _opened;

		public DoorSceneActor(uint uid, DoorSceneActorConfig config, MapCell baseCell, IReadOnlyList<MapCell> extraCells)
			: base(uid, config, baseCell, extraCells)
		{
			_doorConfig = config;
		}

		public override bool CanInteract => !_opened;

		public override IReadOnlyList<SpriteSlice> CurrentSlices
			=> _opened ? _doorConfig.openedSlices : _doorConfig.baseSlices;

		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= LevelContainer.Instance.Resolve<IEventBus>();

		private IRegionService _regionService;
		private IRegionService RegionService => _regionService ??= LevelContainer.Instance.Resolve<IRegionService>();

		public override void Interact()
		{
			if (!CanInteract) return;
			if (RegionService.IsRegionUnlocked(_doorConfig.regionId)) return;

			_opened = true;
			BlockMovement.Clear();
			BlocksVision = false;

			RegionService.UnlockRegion(_doorConfig.regionId);

			EventBus.Publish(new SceneActorVisualChangedEvent(Uid));
			EventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.Interact));
		}
	}
}

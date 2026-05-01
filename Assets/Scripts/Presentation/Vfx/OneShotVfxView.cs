using Core.Events;
using Core.Log;
using Data.Runtime.Events.Vfx;
using Presentation.Bootstrap;
using Systems.Interfaces;
using UnityEngine;

namespace Presentation.Vfx
{
	public class OneShotVfxView : MonoBehaviour
	{
		private IEventBus _eventBus;
		private ICoordinateConverter _coordConverter;

		public void Initialize(ServiceContainer services)
		{
			_eventBus = services.Resolve<IEventBus>();
			_coordConverter = services.Resolve<ICoordinateConverter>();

			_eventBus.Subscribe<OneShotVfxEvent>(OnOneShotVfx);

			this.Log("Initialized");
		}

		private void OnDestroy() => _eventBus?.Unsubscribe<OneShotVfxEvent>(OnOneShotVfx);

		private void OnOneShotVfx(OneShotVfxEvent e)
		{
			if (!e.Prefab) return;

			var worldPos = _coordConverter.CellToWorld(e.Cell);
			var go = Instantiate(e.Prefab, worldPos, Quaternion.identity, transform);

			go.name = $"OneShot_{e.Prefab.name}_f{Time.frameCount}";
		}
	}
}

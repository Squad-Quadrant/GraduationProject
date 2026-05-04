using Core.Events;
using Core.Log;
using Data.Runtime.Events.Vfx;
using Data.Runtime.Events.View;
using Presentation.Audio;
using Presentation.Bootstrap;
using Presentation.Unit;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using UnityEngine;

namespace Presentation.Vfx
{
	public class ThrowPresenter : MonoBehaviour
	{
		[Title("References")]
		[SerializeField]
		private Transform projectileContainer;

		[Title("Settings")]
		[SerializeField, MinValue(0.1f), Tooltip("投掷物飞行时长（秒）")]
		private float flightDuration = 0.6f;

		[SerializeField, MinValue(0f), Tooltip("抛物线最高点偏移（世界单位）")]
		private float arcHeight = 1.5f;

		[SerializeField]
		private Vector3 launchOffset = new(0f, 0.5f, 0f);

		[SerializeField]
		private string throwStartEventName = "throw start";

		private IEventBus _eventBus;
		private ICoordinateConverter _coordConverter;
		private UnitViewManager _unitViewManager;
		private AudioService _audioService;

		public void Initialize(ServiceContainer services)
		{
			_eventBus = services.Resolve<IEventBus>();
			_coordConverter = services.Resolve<ICoordinateConverter>();
			_unitViewManager = services.Resolve<UnitViewManager>();
			_audioService = services.Resolve<AudioService>();

			if (!projectileContainer) projectileContainer = transform;

			_eventBus.Subscribe<ThrowEvent>(OnThrow);

			this.Log("Initialized");
		}

		private void OnDestroy() => _eventBus?.Unsubscribe<ThrowEvent>(OnThrow);

		private void OnThrow(ThrowEvent e)
		{
			if (!_unitViewManager.TryGetView(e.OwnerUnitId, out var unitView))
			{
				this.LogWarning($"No UnitView for '{e.OwnerUnitId}'. Completing immediately.");
				PublishComplete(e.OwnerUnitId);
				return;
			}

			var fromCell = _coordConverter.WorldToCell(unitView.transform.position);
			var dir = e.TargetCell - fromCell;
			if (dir != Vector2Int.zero) unitView.SetFacing(dir);

			if (e.ItemConfig.clipWhenThrowAnimationStarted) _audioService.PlaySfx(e.ItemConfig.clipWhenThrowAnimationStarted);
			unitView.PlayAction("throw", () => unitView.PlayAction("idle"));
			unitView.ListenForSpineEvent(throwStartEventName, () => SpawnProjectile(e, unitView));
		}

		private void SpawnProjectile(ThrowEvent e, UnitView unitView)
		{
			if (!e.ProjectilePrefab)
			{
				this.LogWarning($"ProjectilePrefab is null for '{e.OwnerUnitId}'. Skipping arc, completing.");
				PublishComplete(e.OwnerUnitId);
				return;
			}

			var fromWorld = unitView.transform.position + launchOffset;
			var toWorld = _coordConverter.CellToWorld(e.TargetCell);

			var go = Instantiate(e.ProjectilePrefab, fromWorld, Quaternion.identity, projectileContainer);
			go.name = $"Projectile_{e.ProjectilePrefab.name}_f{Time.frameCount}";

			var projectile = go.GetComponent<ThrowProjectileView>();
			if (!projectile)
			{
				this.LogError($"Prefab '{e.ProjectilePrefab.name}' missing ThrowProjectileView. Completing immediately.");
				Destroy(go);
				PublishComplete(e.OwnerUnitId);
				return;
			}

			if (e.ItemConfig.clipWhenProjectileGenerated) _audioService.PlaySfx(e.ItemConfig.clipWhenProjectileGenerated);
			projectile.Launch(fromWorld, toWorld, flightDuration, arcHeight, onLanded: () =>
			{
				PublishComplete(e.OwnerUnitId);
				if (go) Destroy(go);
			});
		}

		private void PublishComplete(string ownerUnitId)
		{
			_eventBus.Publish(new PresentationCompleteEvent(
				category: EPresentationCategory.Animation,
				type: PresentationType.Animation.Throw,
				entityId: ownerUnitId));
		}
	}
}

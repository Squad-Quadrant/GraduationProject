using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Buff;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.Unit;
using Data.Runtime.Events.View;
using Data.Runtime.Events.Vision;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Damage;
using Systems.Interfaces;
using Systems.Map;
using Systems.Map.Config;
using Systems.Unit;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Config;
using Systems.Vision;
using UnityEngine;

namespace Presentation.Unit
{
	public class UnitViewManager : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required] private UnitView unitViewPrefab;
		[SerializeField] private Transform unitContainer;

		private IEventBus _eventBus;
		private ICoordinateConverter _coordConverter;
		private IUnitService _unitService;
		private IVisionService _visionService;
		private IMapService _mapService;

		private readonly Dictionary<string, UnitView> _views = new(); // [unitId, view]

		private string _lastHoveredUnitId;

		public void Initialize(ServiceContainer services)
		{
			_eventBus = services.Resolve<IEventBus>();
			_coordConverter = services.Resolve<ICoordinateConverter>();
			_unitService = services.Resolve<IUnitService>();
			_visionService = services.Resolve<IVisionService>();
			_mapService = services.Resolve<IMapService>();

			if (!unitContainer) unitContainer = transform;

			_eventBus.Subscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Subscribe<UnitDestroyedEvent>(OnUnitDestroyed);
			_eventBus.Subscribe<UnitMovedEvent>(OnUnitMoved);
            _eventBus.Subscribe<DealDamageEvent>(OnUnitAttacked);
            _eventBus.Subscribe<DamageAppliedEvent>(OnUnitBeHit);
            _eventBus.Subscribe<VisionChangedEvent>(OnVisionChanged);
            _eventBus.Subscribe<UnitReloadedEvent>(OnUnitReload);
            _eventBus.Subscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);
            _eventBus.Subscribe<NoticeUnitVisionToUpdateEvent>(OnNoticeUnitVisionToUpdate);
            _eventBus.Subscribe<PointerHoverEvent>(OnPointerHover);
            _eventBus.Subscribe<RangeDisplayEvent>(OnRangeDisplay);
            _eventBus.Subscribe<DisplayAttackContextEvent>(OnDisplayAttackContext);
            _eventBus.Subscribe<BuffAttachedEvent>(OnBuffAttach);
            _eventBus.Subscribe<BuffLostEvent>(OnBuffLost);
            _eventBus.Subscribe<BuffTurnEvent>(OnBuffTurn);
			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			_eventBus.Unsubscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Unsubscribe<UnitDestroyedEvent>(OnUnitDestroyed);
			_eventBus.Unsubscribe<UnitMovedEvent>(OnUnitMoved);
            _eventBus.Unsubscribe<DealDamageEvent>(OnUnitAttacked);
            _eventBus.Unsubscribe<DamageAppliedEvent>(OnUnitBeHit);
            _eventBus.Unsubscribe<VisionChangedEvent>(OnVisionChanged);
            _eventBus.Unsubscribe<UnitReloadedEvent>(OnUnitReload);
            _eventBus.Unsubscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);
            _eventBus.Unsubscribe<NoticeUnitVisionToUpdateEvent>(OnNoticeUnitVisionToUpdate);
            _eventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);
            _eventBus.Unsubscribe<RangeDisplayEvent>(OnRangeDisplay);
            _eventBus.Unsubscribe<DisplayAttackContextEvent>(OnDisplayAttackContext);
            _eventBus.Unsubscribe<BuffAttachedEvent>(OnBuffAttach);
            _eventBus.Unsubscribe<BuffLostEvent>(OnBuffLost);
            _eventBus.Unsubscribe<BuffTurnEvent>(OnBuffTurn);

			// destroy all remaining views to clean up the scene
			foreach (var view in _views.Values.Where(view => view && view.gameObject))
				Destroy(view.gameObject);
			_views.Clear();
		}

		public UnitView GetView(string unitId)
		{
			_views.TryGetValue(unitId, out var view);
			return view;
		}

		public bool TryGetView(string unitId, out UnitView view) =>
			_views.TryGetValue(unitId, out view);

		public bool HasView(string unitId) => _views.ContainsKey(unitId);

		// spawn a unit view for the given unitId
		private void OnUnitCreated(UnitCreatedEvent e)
		{
			if (e.Unit == null)
			{
				this.LogError("UnitCreatedEvent has null Unit");
				return;
			}

			var unit = e.Unit;

			if (_views.ContainsKey(unit.id))
			{
				this.LogWarning($"View for unit '{unit.id}' already exists");
				return;
			}

			if (!unit.animationConfig)
			{
				this.LogError($"Unit '{unit.id}' has null animationConfig");
				return;
			}

			var viewInstance = CreateUnitViewInstance(unit);
			_views[unit.id] = viewInstance;
			viewInstance.SetVisible(false);
			_eventBus.Publish(new UnitViewSpawnedEvent(unit.id, viewInstance));
			
			var currentWeapon = e.Unit.CurrentWeaponContainer;
			if (!currentWeapon.IsNullOrEmpty())
			{
				var config = currentWeapon.Config as WeaponConfig;
				if (config)
				{
					viewInstance.SetWeaponSkin(config.spineName);
					viewInstance.SetGrip(config.gripType);
					viewInstance.SetWeaponAnimKey(config.animKey);
				}
			}
			this.Log($"Unit view created for unit '{unit.id}'");
		}

		private void OnUnitDestroyed(UnitDestroyedEvent e)
		{
			if (e.Unit == null)
			{
				this.LogError("UnitDestroyedEvent has null Unit");
				return;
			}

			var unitId = e.Unit.id;

			if (_lastHoveredUnitId == unitId)
			{
				if (_views.TryGetValue(unitId, out var hoveredView) && hoveredView)
					hoveredView.SetOutline(false);
				_lastHoveredUnitId = null;
			}

			if (!_views.Remove(unitId, out var view))
			{
				this.LogWarning($"No view found for destroyed unit '{unitId}'.");
				return;
			}

			view.CancelMovement();
			view.PlayAction("hitdown", () =>
			{
				_eventBus.Publish(new PresentationCompleteEvent(
					category: EPresentationCategory.Animation,
					type: PresentationType.Animation.Death,
					entityId: unitId));

				if (!view) return;
				view.PlayAction("dead");
				view.FadeOut(() =>
				{
					_eventBus.Publish(new UnitViewDespawnedEvent(unitId));
					if (view && view.gameObject)
						Destroy(view.gameObject);
				});
			});

			_visionService.ClearSpottedMark(unitId);

			this.Log($"View destroying for '{unitId}'.");
		}

		private void OnUnitMoved(UnitMovedEvent e)
		{
			if (e.Unit == null)
			{
				this.LogError("UnitMovedEvent has null Unit");
				return;
			}

			if (!_views.TryGetValue(e.Unit.id, out var view))
			{
				this.LogWarning($"No view found for moved unit '{e.Unit.id}'.");
				return;
			}

			if (e.Path == null || e.Path.Count < 2)
			{
				this.LogWarning($"Invalid or empty path for unit movement of '{e.Unit.id}'. Skipping animation.");
				view.transform.position = _coordConverter.CellToWorld(e.ToPosition);
				view.PlayAction("idle");
				_eventBus.Publish(new PresentationCompleteEvent(
					category: EPresentationCategory.Animation,
					type: PresentationType.Animation.Move,
					entityId: e.Unit.id
				));
				return;
			}

			var movingUnit = e.Unit;
			view.Move(
				e.Path,
				onStep: cell =>
				{
					if (e.Unit.faction != EUnitFaction.Player)
					{
						view.SetVisible(_visionService.IsCellVisible(cell));
						return;
					}
					_visionService.UpdateUnitVision(movingUnit.id, cell, movingUnit.visionRange);
				},
				onComplete: () =>
				{
					if (e.Unit.faction != EUnitFaction.Player)
						view.SetVisible(_visionService.IsCellVisible(movingUnit.position));
					// else
					// 	_visionService.UpdateUnitVision(movingUnit.id, movingUnit.position, movingUnit.visionRange); // 确保结束时视野正确更新

					// 移动到矮墙周围自动蹲下
					var neighborCells = _mapService.Data.GetNeighborWalls(movingUnit.position);
					view.SetStance(neighborCells.Any(wall => wall.Type == WallType.LowWall) ? EUnitStance.Bend : EUnitStance.Stand);

					_eventBus.Publish(new PresentationCompleteEvent(
						category: EPresentationCategory.Animation,
						type: PresentationType.Animation.Move,
						entityId: e.Unit.id
					));
				});
		}

        private void OnNoticeUnitVisionToUpdate(NoticeUnitVisionToUpdateEvent e)
        {
            if (e.Unit == null)
            {
                this.LogError("NoticeUnitVisionToChangeEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(e.Unit.id, out var view))
            {
                this.LogWarning($"No view found for visionChanging unit '{e.Unit.id}'.");
                return;
            }
            _visionService.UpdateUnitVision(e.Unit.id, e.Unit.position, e.Unit.visionRange);
        }
        

        private UnitView CreateUnitViewInstance(Systems.Unit.Unit unit)
		{
			var viewObj = Instantiate(unitViewPrefab.gameObject, unitContainer);
			viewObj.name = $"UnitView_{unit.name}_{unit.id}";

			var view = viewObj.GetComponent<UnitView>();
			view.Initialize(
				unitId: unit.id,
				config: unit.animationConfig,
				coordConverter: _coordConverter,
				skeletonDataAsset: unit.skeletonDataAsset,
				frontBodySkinName: unit.frontBodySkin,
				backBodySkinName: unit.backBodySkin,
				initialGridPos: unit.position,
				weaponSkinName: unit.defaultWeaponSkin);

			return view;
		}
        
        private void OnUnitAttacked(DealDamageEvent e)
        {
	        var info = e.Info;
	        if (info.DamageType != DamageType.Bullet) return;

	        var attacker = info.Attacker as Systems.Unit.Unit;
            if (attacker == null)
            {
                this.LogError("UnitAttackedEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(attacker.id, out var view))
            {
                this.LogWarning($"No view found for attacked unit '{attacker.id}'.");
                return;
            }

            view.PlayAction("shoot", () =>
            {
                _eventBus.Publish(new PresentationCompleteEvent(
                    category: EPresentationCategory.Animation,
                    type: PresentationType.Animation.Attack,
                    entityId: attacker.id
                ));
                view.PlayAction("idle");
            });
        }
        
        private void OnUnitBeHit(DamageAppliedEvent e)
        {
            var defender = e.Context.Defender;
            if (defender == null)
            {
                this.LogError("UnitBeHitEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(defender.id, out var view))
            {
                this.LogWarning($"No view found for hit unit '{defender.id}'.");
                return;
            }
            if (!e.Context.isMiss)
            {
                view.PlayAction("beHit", () =>
                {
                    _eventBus.Publish(new PresentationCompleteEvent(
                        category: EPresentationCategory.Animation,
                        type: PresentationType.Animation.BeHit,
                        entityId: defender.id
                    ));
                });
            }
        }

        private void OnUnitReload(UnitReloadedEvent e)
        {
            var unit = e.Unit;
            if (unit == null)
            {
                this.LogError("UnitBeHitEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(unit.id, out var view))
            {
                this.LogWarning($"No view found for hit unit '{unit.id}'.");
                return;
            }
            view.PlayAction("reload", () =>
            {
                _eventBus.Publish(new PresentationCompleteEvent(
                    category: EPresentationCategory.Animation,
                    type: PresentationType.Animation.Reload,
                    entityId: unit.id
                ));
            });
        }

        private void OnUnitInfoChanged(UnitInfoChangedEvent e)
        {
            if (e.Unit == null)
            {
                this.LogError("UnitInfoChangedEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(e.Unit.id, out var view))
            {
                this.LogWarning($"No view found for updated unit '{e.Unit.id}'.");
                return;
            }

            var currentWeapon = e.Unit.CurrentWeaponContainer;
            if (!currentWeapon.IsNullOrEmpty())
            {
	            var config = currentWeapon.Config as WeaponConfig;
	            if (!config)
	            {
		            view.SetGrip(config.gripType);
		            view.SetWeaponSkin(config.spineName);
		            view.SetWeaponAnimKey(config.animKey);
	            }
            }
        }

        private void OnBuffAttach(BuffAttachedEvent e)
        {
            var unit = e.Buffable as Systems.Unit.Unit;
            if(unit == null)
            {
                this.LogError("AttachBuffEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(unit.id, out var view))
            {
                this.LogWarning($"No view found for attached unit '{unit.id}'.");
                return;
            }

            var oneShotVFX = e.BuffInfo.OneShotVfxPrefab;
            if (oneShotVFX)
            {
                var go = Instantiate(oneShotVFX, view.transform.position, Quaternion.identity, transform);
                go.name = $"Buff_OneShot_{oneShotVFX.name}_f{Time.frameCount}";
            }
            
            view.OnAttachBuff(e.BuffInfo);
        }

        private void OnBuffLost(BuffLostEvent e)
        {
            var unit = e.Buffable as Systems.Unit.Unit;
            if(unit == null)
            {
                this.LogError("LostBuffEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(unit.id, out var view))
            {
                this.LogWarning($"No view found for attached unit '{unit.id}'.");
                return;
            }
            
            view.OnLostBuff(e.BuffInfo);
        }

        private void OnBuffTurn(BuffTurnEvent e)
        {
            var unit = e.Buffable as Systems.Unit.Unit;
            if(unit == null)
            {
                this.LogError("BuffTurnEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(unit.id, out var view))
            {
                this.LogWarning($"No view found for attached unit '{unit.id}'.");
                return;
            }

            var turnVFX = e.BuffInfo.TurnVfxPrefab;
            if (turnVFX)
            {
                var go = Instantiate(turnVFX, view.transform.position, Quaternion.identity, transform);
                go.name = $"Buff_Turn_{turnVFX.name}_f{Time.frameCount}";
            }
        }

        private void OnVisionChanged(VisionChangedEvent e)
        {
	        foreach (var (unitId, view) in _views)
	        {
		        if (!view) continue;

		        if (view.IsMoving)
		        {
			        view.SetVisible(true);
			        continue;
		        }

		        if (!_unitService.TryGetUnit(unitId, out var unit))
		        {
			        view.SetVisible(false);
			        continue;
		        }

		        view.SetVisible(e.VisibleCells.Contains(unit.position));
	        }
        }

        private void OnPointerHover(PointerHoverEvent e)
        {
	        if (e.HoveredUnitId == _lastHoveredUnitId) return;

	        if (_lastHoveredUnitId != null
	            && _views.TryGetValue(_lastHoveredUnitId, out var oldView) && oldView)
		        oldView.SetOutline(false);

	        if (e.HoveredUnitId != null
	            && _views.TryGetValue(e.HoveredUnitId, out var newView) && newView)
		        newView.SetOutline(true);

	        _lastHoveredUnitId = e.HoveredUnitId;
        }

        private void OnRangeDisplay(RangeDisplayEvent e)
        {
	        foreach (var pair in _views)
	        {
		        if (!_unitService.TryGetUnit(pair.Key, out var unit)) continue;
		        if (e.RangeType is ERangeType.AreaEffectPreview && e.Cells.Contains(unit.position))
			        pair.Value.StartPulse(e.AreaEffectColor);
		        else
			        pair.Value.StopPulse();
	        }
        }

        private void OnDisplayAttackContext(DisplayAttackContextEvent e) // 暂时借用一下语义，理论上这么写不太好
        {
	        if (e.Context == null)
	        {
		        foreach (var view in _views.Values) view.StopPulse();
		        return;
	        }
	        string targetId = e.Context.Defender.id;
	        var targetView = _views.GetValueOrDefault(targetId);
	        targetView?.StartPulse(Color.red);
        }

		#region Odin Debug

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly, LabelText("Active Views")]
		private int DbgViewCount => _views.Count;

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly]
		[ListDrawerSettings(ShowFoldout = true)]
		private List<string> DbgViewIds => new(_views.Keys);

		#endregion
	}
}

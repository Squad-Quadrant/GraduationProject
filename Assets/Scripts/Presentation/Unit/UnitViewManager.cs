using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Unit;
using Data.Runtime.Events.View;
using Data.Runtime.Events.Vision;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Equipment;
using Systems.Interfaces;
using Systems.Unit;
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

		private readonly Dictionary<string, UnitView> _views = new(); // [unitId, view]

		private HashSet<Vector2Int> _currentVisibleCells = new();

		public void Initialize(ServiceContainer services)
		{
			_eventBus = services.Resolve<IEventBus>();
			_coordConverter = services.Resolve<ICoordinateConverter>();
			_unitService = services.Resolve<IUnitService>();
			_visionService = services.Resolve<IVisionService>();

			if (!unitContainer) unitContainer = transform;

			_eventBus.Subscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Subscribe<UnitDestroyedEvent>(OnUnitDestroyed);
			_eventBus.Subscribe<UnitMovedEvent>(OnUnitMoved);
            _eventBus.Subscribe<UnitAttackedEvent>(OnUnitAttacked);
            _eventBus.Subscribe<DamageAppliedEvent>(OnUnitBeHit);
            _eventBus.Subscribe<VisionChangedEvent>(OnVisionChanged);
            _eventBus.Subscribe<UnitReloadedEvent>(OnUnitReload);
            _eventBus.Subscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);
			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			_eventBus.Unsubscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Unsubscribe<UnitDestroyedEvent>(OnUnitDestroyed);
			_eventBus.Unsubscribe<UnitMovedEvent>(OnUnitMoved);
            _eventBus.Unsubscribe<UnitAttackedEvent>(OnUnitAttacked);
            _eventBus.Unsubscribe<DamageAppliedEvent>(OnUnitBeHit);
            _eventBus.Unsubscribe<VisionChangedEvent>(OnVisionChanged);
            _eventBus.Unsubscribe<UnitReloadedEvent>(OnUnitReload);
            _eventBus.Unsubscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);

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
			
			var currentEquipment = e.Unit.CurrentEquipment;
			if (!currentEquipment.IsNullOrEmpty())
			{
				
				viewInstance.SetWeaponSkin(e.Unit.CurrentEquipment?.Config.SpineName);
				viewInstance.SetGrip(e.Unit.CurrentEquipment.Config.GripType);
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

			if (!_views.Remove(unitId, out var view))
			{
				this.LogWarning($"No view found for destroyed unit '{unitId}'.");
				return;
			}

			view.CancelMovement();
			view.PlayAction("death", () =>
			{
				_eventBus.Publish(new UnitViewDespawnedEvent(unitId));
				if (view && view.gameObject)
					Destroy(view.gameObject);
			});

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
						bool inPlayerSight = _currentVisibleCells != null && _currentVisibleCells.Contains(cell);
						view.SetVisible(inPlayerSight);
						return;
					}
					var visible = _visionService.CalculateVisibleCells(cell, movingUnit.visionRange);
					_eventBus.Publish(new VisionChangedEvent(visible, movingUnit.id));
				},
				onComplete: () =>
				{
					_eventBus.Publish(new PresentationCompleteEvent(
						category: EPresentationCategory.Animation,
						type: PresentationType.Animation.Move,
						entityId: e.Unit.id
					));
				});
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
        
        private void OnUnitAttacked(UnitAttackedEvent e)
        {
            if (e.Attacker == null)
            {
                this.LogError("UnitAttackedEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(e.Attacker.id, out var view))
            {
                this.LogWarning($"No view found for attacked unit '{e.Attacker.id}'.");
                return;
            }

            view.PlayAction("shoot", () =>
            {
                _eventBus.Publish(new PresentationCompleteEvent(
                    category: EPresentationCategory.Animation,
                    type: PresentationType.Animation.Attack,
                    entityId: e.Attacker.id
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

            if (!e.Unit.CurrentEquipment.IsNullOrEmpty())
            {
                var config = e.Unit.CurrentEquipment.Config;
                view.SetGrip(config.GripType);
                view.SetWeaponSkin(config.SpineName);
            }
        }

        private void OnVisionChanged(VisionChangedEvent e)
        {
	        _currentVisibleCells = e.VisibleCells;

	        foreach (var (unitId, view) in _views)
	        {
		        if (!view) continue;

		        if (unitId == e.UnitId)
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

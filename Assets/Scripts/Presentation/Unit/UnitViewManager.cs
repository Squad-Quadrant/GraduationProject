using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Unit;
using Data.Runtime.Events.View;
using Data.Runtime.Events.Vision;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
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
            _eventBus.Subscribe<UnitBeHitEvent>(OnUnitBeHit);
            _eventBus.Subscribe<VisionChangedEvent>(OnVisionChanged);

			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			_eventBus.Unsubscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Unsubscribe<UnitDestroyedEvent>(OnUnitDestroyed);
			_eventBus.Unsubscribe<UnitMovedEvent>(OnUnitMoved);
            _eventBus.Unsubscribe<UnitAttackedEvent>(OnUnitAttacked);
            _eventBus.Unsubscribe<UnitBeHitEvent>(OnUnitBeHit);
            _eventBus.Unsubscribe<VisionChangedEvent>(OnVisionChanged);

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
			viewInstance.gameObject.SetActive(false);

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
            if (e.Unit == null)
            {
                this.LogError("UnitAttackedEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(e.Unit.id, out var view))
            {
                this.LogWarning($"No view found for attacked unit '{e.Unit.id}'.");
                return;
            }

            view.PlayAction("shoot", () =>
            {
                _eventBus.Publish(new PresentationCompleteEvent(
                    category: EPresentationCategory.Animation,
                    type: PresentationType.Animation.Attack,
                    entityId: e.Unit.id
                ));
                view.PlayAction("idle");
            });
        }
        
        private void OnUnitBeHit(UnitBeHitEvent e)
        {
            if (e.Unit == null)
            {
                this.LogError("UnitBeHitEvent has null Unit");
                return;
            }

            if (!_views.TryGetValue(e.Unit.id, out var view))
            {
                this.LogWarning($"No view found for hit unit '{e.Unit.id}'.");
                return;
            }

            view.PlayAction("beHit", () =>
            {
                _eventBus.Publish(new PresentationCompleteEvent(
                    category: EPresentationCategory.Animation,
                    type: PresentationType.Animation.BeHit,
                    entityId: e.Unit.id
                ));
            });
        }

        private void OnVisionChanged(VisionChangedEvent e)
        {
	        foreach (var (unitId, view) in _views)
	        {
		        if (!view) continue;

		        if (unitId == e.UnitId)
		        {
			        view.gameObject.SetActive(true);
			        continue;
		        }

		        if (!_unitService.TryGetUnit(unitId, out var unit))
		        {
			        view.gameObject.SetActive(false);
			        continue;
		        }

		        view.gameObject.SetActive(e.VisibleCells.Contains(unit.position));
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

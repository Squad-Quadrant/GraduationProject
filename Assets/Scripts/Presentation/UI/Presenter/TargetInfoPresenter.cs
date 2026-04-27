using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Presentation.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel;
using Systems.Unit;
using UnityEngine;

namespace Presentation.UI.Presenter
{
	public class TargetInfoPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;
		private readonly IUnitService _unitService;
		private readonly InteractionController  _interactionController;

		private TargetInfoPanel _panel;
		private Vector2Int? _targetCell;

		public TargetInfoPresenter(UIManager uiManager, IEventBus eventBus, IUnitService unitService, InteractionController interactionController)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
			_interactionController = interactionController ?? throw new ArgumentNullException(nameof(interactionController));

			_eventBus.Subscribe<TargetingEvent>(OnTargeting);
			_eventBus.Subscribe<PointerHoverEvent>(OnPointerHover);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<TargetingEvent>(OnTargeting);
			_eventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);
			_panel =  null;
		}

		private void OnTargeting(TargetingEvent e)
		{
			_targetCell = e.TargetCell;

			if (!_targetCell.HasValue)
			{
				_uiManager.Close(_panel);
				_panel = null;
				return;
			}

			var target = _unitService.GetUnitAtPosition(_targetCell.Value);
			if (target == null)
			{
				this.LogWarning($"Target {_targetCell.Value} Not Found");
				return;
			}

			_panel = _uiManager.Open<TargetInfoPanel, Systems.Unit.Unit>(target);
		}

		private void OnPointerHover(PointerHoverEvent e)
		{
			if (_targetCell.HasValue) return;
			if (e.HoveredUnitId == _interactionController.Context.selectedUnit.id) return;

			if (!string.IsNullOrEmpty(e.HoveredUnitId) &&
			    _unitService.TryGetUnit(e.HoveredUnitId, out var target))
			{
				_panel = _uiManager.Open<TargetInfoPanel, Systems.Unit.Unit>(target);
				return;
			}

			if (e.CellPosition.HasValue)
			{
				target = _unitService.GetUnitAtPosition(e.CellPosition.Value);
				if (target != null)
				{
					_panel = _uiManager.Open<TargetInfoPanel, Systems.Unit.Unit>(target);
					return;
				}
			}

			_uiManager.Close(_panel);
			_panel = null;
		}
	}
}

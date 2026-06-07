using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel.UnitInfo;
using Systems.Unit;

namespace Presentation.UI.Presenter
{
	public class UnitInfoPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;
		private readonly IUnitService _unitService;
		private UnitInfoPanel _panel;

		public UnitInfoPresenter(UIManager uiManager, IEventBus eventBus, IUnitService unitService)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));

			_eventBus.Subscribe<UnitSelectedEvent>(OnUnitSelected);
			_eventBus.Subscribe<UnitDeselectedEvent>(OnUnitDeselected);
			_eventBus.Subscribe<UnitInspectedEvent>(OnUnitInspected);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<UnitSelectedEvent>(OnUnitSelected);
			_eventBus.Unsubscribe<UnitDeselectedEvent>(OnUnitDeselected);
			_eventBus.Unsubscribe<UnitInspectedEvent>(OnUnitInspected);
			_panel = null;
		}

		private void OnUnitSelected(UnitSelectedEvent e) => ShowUnit(e.UnitId);

		private void OnUnitInspected(UnitInspectedEvent e) => ShowUnit(e.UnitId);

		private void OnUnitDeselected(UnitDeselectedEvent e)
		{
			if (!_panel) return;

			_uiManager.Close(_panel);
			_panel = null;
		}

		private void ShowUnit(string unitId)
		{
			if (!_unitService.TryGetUnit(unitId, out var unit))
			{
				this.LogWarning($"Unit '{unitId}' not found, cannot show info panel");
				return;
			}
			_panel = _uiManager.Open<UnitInfoPanel, Systems.Unit.Unit>(unit);
		}
	}
}

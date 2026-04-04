using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel;
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

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<UnitSelectedEvent>(OnUnitSelected);
			_eventBus.Unsubscribe<UnitDeselectedEvent>(OnUnitDeselected);
			_panel = null;
		}

		private void OnUnitSelected(UnitSelectedEvent e)
		{
			if (!_unitService.TryGetUnit(e.UnitId, out var unit))
			{
				this.LogWarning($"Unit '{e.UnitId}' not found, cannot show info panel");
				return;
			}

			_panel = _uiManager.Open<UnitInfoPanel, Systems.Unit.Unit>(unit);
		}

		private void OnUnitDeselected(UnitDeselectedEvent e)
		{
			if (!_panel) return;

			_uiManager.Close(_panel);
			_panel = null;
		}
	}
}

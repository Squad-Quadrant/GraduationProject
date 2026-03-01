using System;
using Core.Events;
using Data.Runtime.Events.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel;

namespace Presentation.UI.Presenter
{
	public class ActionMenuPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;
		private ActionMenuPanel _panel;

		public ActionMenuPresenter(UIManager uiManager, IEventBus eventBus)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

			_eventBus.Subscribe<UnitSelectedEvent>(ShowActionMenu);
			_eventBus.Subscribe<UnitDeselectedEvent>(CloseActionMenu);
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<UnitSelectedEvent>(ShowActionMenu);
			_eventBus.Unsubscribe<UnitDeselectedEvent>(CloseActionMenu);
		}

		private void ShowActionMenu(UnitSelectedEvent e)
        {
            _panel = _uiManager.Open<ActionMenuPanel, UnitSelectedEvent>(e);
            if (_panel)
            {
                _panel.SetUnit(e.UnitId);
            }
        }

		private void CloseActionMenu(UnitDeselectedEvent e)
		{
			if (_panel) _uiManager.Close(_panel);
		}
	}
}

using System;
using Core.Events;
using Data.Runtime.Events.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel;

namespace Presentation.UI.Presenter
{
    public class TurnPresenter : IDisposable
    {
        private readonly UIManager _uiManager;
        private readonly IEventBus _eventBus;
        private TurnPanel _panel;

        public TurnPresenter(UIManager uiManager, IEventBus eventBus)
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

        private void ShowActionMenu(UnitSelectedEvent e) => _panel = _uiManager.Open<TurnPanel>();

        private void CloseActionMenu(UnitDeselectedEvent e)
        {
            if (_panel) _uiManager.Close(_panel);
        }
    }
}
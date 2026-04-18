using System;
using Core.Events;
using Core.FSM;
using Core.Log;
using Presentation.UI.Core;
using Presentation.UI.Panel.TacticalItemMenu;
using Systems.Interaction;

namespace Presentation.UI.Presenter
{
	public class TacticalItemMenuPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;
		private TacticalItemMenuPanel _panel;

		public TacticalItemMenuPresenter(UIManager uiManager, IEventBus eventBus)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_eventBus.Subscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);

			this.Log("Initialized");
		}

		public void Dispose() => _eventBus.Unsubscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);

		private void OnStateChanged(StateChangedEvent<InteractionContext> e)
		{
			var current = e.CurrentState?.Name;
			var previous = e.PreviousState?.Name;

			if (previous == InteractionStates.ItemSelection && _panel)
			{
				_uiManager.Close<TacticalItemMenuPanel>();
				_panel = null;
			}

			if (current == InteractionStates.ItemSelection)
			{
				var unit = e.Context.selectedUnit;
				if (unit == null)
				{
					this.LogError("Entered ItemSelection without selectedUnit. Panel not opened.");
					return;
				}
				_panel = _uiManager.Open<TacticalItemMenuPanel, Systems.Unit.Unit>(unit);
			}
		}
	}
}

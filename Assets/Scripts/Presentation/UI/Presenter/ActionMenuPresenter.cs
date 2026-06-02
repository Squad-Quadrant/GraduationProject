using System;
using Core.Events;
using Core.FSM;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Presentation.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel.ActionMenu;
using Systems.Interaction;

namespace Presentation.UI.Presenter
{
	public class ActionMenuPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;
		private readonly InteractionController _interaction;

		private ActionMenuPanel _panel;

		public ActionMenuPresenter(UIManager uiManager, IEventBus eventBus, InteractionController interaction)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));

			_eventBus.Subscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);
			// _eventBus.Subscribe<UnitInspectedEvent>(OnUnitInspected);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);
			// _eventBus.Unsubscribe<UnitInspectedEvent>(OnUnitInspected);
		}

        private void OnStateChanged(StateChangedEvent<InteractionContext> e)
        {
	        var current = e.CurrentState?.Name;
	        var previous = e.PreviousState?.Name;

	        if (previous is InteractionStates.UnitSelected or InteractionStates.UnitInspect && _panel)
	        {
		        _uiManager.Close<ActionMenuPanel>();
		        _panel = null;
	        }

	        if (current is InteractionStates.UnitSelected or InteractionStates.UnitInspect)
	        {
		        if (!_panel)
					_panel = _uiManager.Open<ActionMenuPanel, Systems.Unit.Unit>(e.Context.selectedUnit);

		        if (current == InteractionStates.UnitInspect)
			        _panel.ShowLocked(e.Context.inspectedUnit);
		        else
			        _panel.ShowActions(e.Context.selectedUnit);
	        }
        }
    }
}

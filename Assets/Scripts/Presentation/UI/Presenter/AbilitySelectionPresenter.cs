using System;
using Core.Events;
using Core.FSM;
using Core.Log;
using Data.Runtime;
using Presentation.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel.SkillMenu;
using Presentation.UI.Panel.TacticalItemMenu;
using Systems.Interaction;

namespace Presentation.UI.Presenter
{
	public class AbilitySelectionPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;
		private readonly InteractionController _interactionController;

		private TacticalItemMenuPanel _tacticalItemPanel;
		private SkillMenuPanel _skillPanel;

		public AbilitySelectionPresenter(UIManager uiManager, IEventBus eventBus, InteractionController interactionController)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_interactionController = interactionController ?? throw new ArgumentNullException(nameof(interactionController));

			_eventBus.Subscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);

			this.Log("Initialized");
		}

		public void Dispose() => _eventBus.Unsubscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);

		private void OnStateChanged(StateChangedEvent<InteractionContext> e)
		{
			var current = e.CurrentState?.Name;
			var previous = e.PreviousState?.Name;

			if (previous == InteractionStates.AbilitySelection)
			{
				if (_tacticalItemPanel)
				{
					_uiManager.Close<TacticalItemMenuPanel>();
					_tacticalItemPanel = null;
				}

				if (_skillPanel)
				{
					_uiManager.Close<SkillMenuPanel>();
					_skillPanel = null;
				}
			}

			if (current == InteractionStates.AbilitySelection)
			{
				var unit = e.Context.selectedUnit;
				if (unit == null)
				{
					this.LogError("Entered AbilitySelection without selectedUnit. Panel not opened.");
					return;
				}

				switch (e.Context.currentAction)
				{
					case EActionType.UseTacticalItem:
						_tacticalItemPanel = _uiManager.Open<TacticalItemMenuPanel, Systems.Unit.Unit>(unit);
						_tacticalItemPanel.Init(_interactionController);
						break;

					case EActionType.UseSkill:
						_skillPanel = _uiManager.Open<SkillMenuPanel, Systems.Unit.Unit>(unit);
						break;
				}
			}
		}
	}
}

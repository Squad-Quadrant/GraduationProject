using System;
using System.Linq;
using Core.Events;
using Core.FSM;
using Core.Log;
using Data.Runtime;
using Presentation.UI.Core;
using Presentation.UI.Panel.SkillMenu;
using Presentation.UI.Panel.TacticalItemMenu;
using Systems.Interaction;
using Systems.Unit.Equipment;

namespace Presentation.UI.Presenter
{
	public class AbilitySelectionPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;

		private TacticalItemMenuPanel _tacticalItemPanel;
		private SkillMenuPanel _skillPanel;

		public AbilitySelectionPresenter(UIManager uiManager, IEventBus eventBus)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

			_eventBus.Subscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);

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

		private void OnStateChanged(StateChangedEvent<InteractionContext> e)
		{
			var current = e.CurrentState?.Name;
			var previous = e.PreviousState?.Name;

			if (previous == InteractionStates.AbilityTargeting)
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

			if (current != InteractionStates.AbilityTargeting) return;

			var ctx = e.Context;
			if (ctx.selectedUnit == null)
			{
				this.LogError("Entered AbilityTargeting without selectedUnit. Panel not opened.");
				return;
			}

			switch (ctx.currentAction)
			{
				case EActionType.UseTacticalItem:
					OpenTacticalItemPanel(ctx);
					break;

				case EActionType.UseSkill:
					_skillPanel = _uiManager.Open<SkillMenuPanel, Systems.Unit.Unit>(ctx.selectedUnit);
					break;

				default:
					this.LogError($"Entered AbilityTargeting with unexpected currentAction: {ctx.currentAction}");
					break;
			}
		}

		private void OpenTacticalItemPanel(InteractionContext ctx)
		{
			var container = FindTacticalItemContainerFor(ctx);
			if (container == null)
			{
				this.LogError(
					$"Cannot find tactical item container for PendingAbility {ctx.PendingAbility?.GetType().Name ?? "null"}. Panel not opened.");
				return;
			}

			_tacticalItemPanel = _uiManager.Open<TacticalItemMenuPanel, EquipmentContainer>(container);
		}

		private static EquipmentContainer FindTacticalItemContainerFor(InteractionContext ctx)
		{
			if (ctx.PendingAbility == null) return null;

			var items = ctx.selectedUnit?.TacticalItems;
			return items?.FirstOrDefault(container =>
				!container.IsNullOrEmpty() &&
				ReferenceEquals(container.Logic, ctx.PendingAbility));
		}
	}
}

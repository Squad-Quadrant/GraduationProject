using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Presentation.Interaction;
using Systems.Interaction.States;
using Systems.Turn;

namespace Systems.BattleFlow
{
	public class PlayerTurnController : ITurnController
	{
		private readonly IEventBus _eventBus;
		private readonly ITurnService _turnService;
		private readonly InteractionController _fsm;

		public PlayerTurnController(IEventBus eventBus, ITurnService turnService, InteractionController fsm)
		{
			_eventBus = eventBus;
			_turnService = turnService;
			_fsm = fsm;
		}

		public void BeginTurn(ITurnUnit turnUnit)
		{
			if (turnUnit is not Unit.Unit unit)
			{
				this.LogError("TurnUnit is not 'Unit.Unit', skipping turn");
				_turnService.EndUnitTurn();
				return;
			}

			this.Log($"Player control: '{unit.name}'");
			_fsm.Context.selectedUnit = unit;
			_eventBus.Publish(new UnitSelectedEvent(unit.id, unit.position));
			_fsm.StateMachine.ChangeState<UnitSelectedState>();
		}
	}
}

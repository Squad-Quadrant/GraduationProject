using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Presentation.Interaction;
using Systems.Interaction.States;

namespace Systems.GamePlay
{
	public class PlayerTurnController : ITurnController
	{
		private readonly IEventBus _eventBus;
		private readonly InteractionController _fsm;

		public PlayerTurnController(IEventBus eventBus, InteractionController fsm)
		{
			_eventBus = eventBus;
			_fsm = fsm;
		}

		public void BeginTurn(Unit.Unit unit)
		{
			this.Log($"Player control: '{unit.name}'");
			_fsm.Context.selectedUnit = unit;
			_eventBus.Publish(new UnitSelectedEvent(unit.id, unit.position));
			_fsm.StateMachine.ChangeState<UnitSelectedState>();
		}
	}
}

using Core.Events;

namespace Core.FSM
{
	public readonly struct StateChangedEvent<TContext> : IEvent
	{
		public StateMachine<TContext> StateMachine { get; }
		public IState<TContext> PreviousState { get; }
		public IState<TContext> CurrentState { get; }
		public TContext Context { get; }

		public StateChangedEvent(
			StateMachine<TContext> stateMachine,
			IState<TContext> previousState,
			IState<TContext> currentState,
			TContext context)
		{
			StateMachine = stateMachine;
			PreviousState = previousState;
			CurrentState = currentState;
			Context = context;
		}

		public override string ToString()
		{
			var prevName = PreviousState?.Name ?? "None";
			var currName = CurrentState?.Name ?? "None";
			return $"StateChanged: {prevName} -> {currName}";
		}
	}
}

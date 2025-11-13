namespace Core.FSM
{
	public interface ITransition<in TContext>
	{
		string Name { get; }
		IState<TContext> From { get; }
		IState<TContext> To { get; }
		int Priority { get; }
		bool ShouldTransition(TContext context);
		void OnTransition(TContext context);
	}
}

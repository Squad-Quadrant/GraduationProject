namespace Core.FSM
{
	public interface IState<in TContext>
	{
		string Name { get; }
		void OnEnter(TContext context);
		void OnUpdate(TContext context, float deltaTime);
		void OnExit(TContext context);
	}
}

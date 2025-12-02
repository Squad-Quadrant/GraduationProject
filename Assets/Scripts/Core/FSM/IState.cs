namespace Core.FSM
{
	public interface IState<in TContext>
	{
		string Name { get; }
		void OnEnter(TContext ctx);
		void OnUpdate(TContext ctx, float deltaTime);
		void OnExit(TContext ctx);
	}
}

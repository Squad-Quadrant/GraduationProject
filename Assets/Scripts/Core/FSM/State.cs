namespace Core.FSM
{
	/// <summary>
	/// Base class for defining states in a finite state machine.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	public abstract class State<TContext> : IState<TContext>
	{
		public string Name { get; }

		protected State(string name = null) => Name = name ?? GetType().Name;

		public virtual void OnEnter(TContext context) { }

		public virtual void OnUpdate(TContext context, float deltaTime) { }

		public virtual void OnExit(TContext context) { }
	}
}

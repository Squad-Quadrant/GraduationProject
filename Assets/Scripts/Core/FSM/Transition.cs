using System;
using System.Linq;

namespace Core.FSM
{
	/// <summary>
	/// Base class for defining transitions between states in a finite state machine.
	/// </summary>
	public abstract class Transition<TContext> : ITransition<TContext>
	{
		public string Name { get; protected set; }
		public IState<TContext> From { get; }
		public IState<TContext> To { get; }
		public int Priority { get; } // Higher priority transitions are evaluated first

		protected Transition(IState<TContext> from, IState<TContext> to, int priority = 0, string name = null)
		{
			From = from;
			To = to ?? throw new ArgumentNullException(nameof(to));
			Priority = priority;
			Name = name ?? $"{From?.Name ?? "Any"} -> {To.Name}";
		}

		public abstract bool ShouldTransition(TContext context);

		public virtual void OnTransition(TContext context)
		{
			// Optional override for transition logic
		}
	}

	/// <summary>
	/// Transition based on a condition function.
	/// </summary>
	public class FuncTransition<TContext> : Transition<TContext>
	{
		private readonly Func<TContext, bool> _condition;
		private readonly Action<TContext> _onTransition;

		public FuncTransition(
			IState<TContext> from,
			IState<TContext> to,
			Func<TContext, bool> condition,
			Action<TContext> onTransition = null,
			int priority = 0,
			string name = null)
			: base(from, to, priority, name)
		{
			_condition = condition ?? throw new ArgumentNullException(nameof(condition));
			_onTransition = onTransition;
		}

		public override bool ShouldTransition(TContext context) => _condition(context);

		public override void OnTransition(TContext context) => _onTransition?.Invoke(context);
	}

	public class AnyStateTransition<TContext> : FuncTransition<TContext>
	{
		public AnyStateTransition(
			IState<TContext> to,
			Func<TContext, bool> condition,
			Action<TContext> onTransition = null,
			int priority = 0,
			string name = null)
			: base(null, to, condition, onTransition, priority, name)
		{
		}
	}

	public class ConditionalTransition<TContext> : Transition<TContext>
	{
		private readonly Func<TContext, bool>[] _conditions;
		private readonly bool _requireAll; // true = AND, false = OR

		public ConditionalTransition(
			IState<TContext> fromState,
			IState<TContext> toState,
			bool requireAll,
			int priority = 0,
			params System.Func<TContext, bool>[] conditions)
			: base(fromState, toState, priority)
		{
			_conditions = conditions;
			_requireAll = requireAll;
		}

		public override bool ShouldTransition(TContext context)
		{
			return _requireAll ?
				_conditions.All(condition => condition(context)) :
				_conditions.Any(condition => condition(context));
		}
	}
}

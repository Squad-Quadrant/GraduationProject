using Core.Events;
using Systems.Damage;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct DisplayAttackContextEvent : IEvent
	{
		public DamageExecutingContext Context { get; }

		private DisplayAttackContextEvent(DamageExecutingContext context) => Context = context;

		public static DisplayAttackContextEvent Valid(DamageExecutingContext context) => new(context);

		public static DisplayAttackContextEvent Invalid() => new(null);
	}
}

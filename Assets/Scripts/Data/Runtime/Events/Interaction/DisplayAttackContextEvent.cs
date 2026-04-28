using Core.Events;
using Systems.Damage;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct DisplayAttackContextEvent : IEvent
	{
		public DamageExecutingContext Context { get; }
		public string OwnerID { get; }

		private DisplayAttackContextEvent(DamageExecutingContext context, string ownerID)
		{
			Context = context;
			OwnerID = ownerID;
		}

		public static DisplayAttackContextEvent Valid(DamageExecutingContext context, string ownerId) => new(context, ownerId);

		public static DisplayAttackContextEvent Invalid() => new(null, string.Empty);
	}
}

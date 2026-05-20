using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct UnitAttackFiredEvent : IEvent
	{
		public string AttackerId { get; }
		public string TargetId   { get; }

		public UnitAttackFiredEvent(string attackerId, string targetId)
		{
			AttackerId = attackerId;
			TargetId   = targetId;
		}

		public override string ToString() => $"[AttackFired] {AttackerId} → {TargetId}";
	}
}

using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct UnitAttackStartedEvent : IEvent
	{
		public string AttackerId { get; }
		public string TargetId   { get; }

		public UnitAttackStartedEvent(string attackerId, string targetId)
		{
			AttackerId = attackerId;
			TargetId   = targetId;
		}

		public override string ToString() => $"[UnitAttackStarted] {AttackerId} → {TargetId}";
	}
}

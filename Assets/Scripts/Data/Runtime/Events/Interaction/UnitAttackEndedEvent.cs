using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct UnitAttackEndedEvent : IEvent
	{
		public string AttackerId { get; }
		public string TargetId   { get; }

		public UnitAttackEndedEvent(string attackerId, string targetId)
		{
			AttackerId = attackerId;
			TargetId   = targetId;
		}

		public override string ToString() => $"[UnitAttackEnded] {AttackerId} → {TargetId}";
	}
}

using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct DisplayHitPercentEvent : IEvent
	{
		public readonly bool IsValid;
		public readonly int HitPercent;

		private DisplayHitPercentEvent(int hitPercent, bool isValid)
		{
			HitPercent = hitPercent;
			IsValid = isValid;
		}

		public static DisplayHitPercentEvent Valid(int hitPercent) => new(hitPercent, true);
		public static DisplayHitPercentEvent Invalid() => new(0, false);
	}
}

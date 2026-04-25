using System.Collections.Generic;
using Core.Events;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct DisplayHitPercentEvent : IEvent
	{
		public readonly bool IsValid;
		public readonly int HitPercent;
		public readonly List<(string, string)> HitRateInfluences;

		private DisplayHitPercentEvent(int hitPercent, bool isValid,  List<(string, string)> hitRateInfluences)
		{
			HitPercent = hitPercent;
			IsValid = isValid;
			HitRateInfluences = hitRateInfluences;
		}

		public static DisplayHitPercentEvent Valid(int hitPercent, List<(string, string)> hitRateInfluences)
		{
			return new DisplayHitPercentEvent(hitPercent, true, hitRateInfluences);
		}
		public static DisplayHitPercentEvent Invalid() => new(0, false, null);
	}
}

using Core.Events;

namespace Data.Runtime.Events.AreaEffect
{
	public readonly struct AreaEffectRegisteredEvent : IEvent
	{
		public Systems.AreaEffect.AreaEffect Effect { get; }

		public AreaEffectRegisteredEvent(Systems.AreaEffect.AreaEffect effect) => Effect = effect;

		public override string ToString() => $"[AreaEffectRegistered] {Effect}";
	}
}

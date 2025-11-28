using Core.Events;

namespace Data.Runtime.Events.UI
{
	public readonly struct ActionCancelledEvent : IEvent
	{
		public EActionType CancelledAction { get; }

		public ActionCancelledEvent(EActionType cancelledAction) => CancelledAction = cancelledAction;

		public override string ToString() => $"[ActionCancelled] {CancelledAction}";
	}
}

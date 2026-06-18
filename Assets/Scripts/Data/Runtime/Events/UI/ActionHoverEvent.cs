using Core.Events;

namespace Data.Runtime.Events.UI
{
	public readonly struct ActionHoverEvent : IEvent
	{
		public EActionType ActionType { get; }

		public int Payload { get; }

		public bool IsEntering { get; }

		public ActionHoverEvent(EActionType actionType, int payload, bool isEntering)
		{
			ActionType = actionType;
			Payload = payload;
			IsEntering = isEntering;
		}

		public override string ToString() => $"[ActionHover] {ActionType}, Payload: {Payload}, Entering: {IsEntering}";
	}
}

using Core.Events;

namespace Data.Runtime.Events.UI
{
	public readonly struct ActionSelectedEvent : IEvent
	{
		public EActionType ActionType { get; }

		// Attack: 0 = 普通射击, 1 = 精确射击
		// UseTacticalItem: 战术道具槽位索引 (0/1/2)
		public int Payload { get; }

		public ActionSelectedEvent(EActionType actionType, int payload = 0)
		{
			ActionType = actionType;
			Payload = payload;
		}

		public override string ToString() => $"[ActionSelected] {ActionType}, Payload: {Payload}";
	}
}

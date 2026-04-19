using Core.Events;


namespace Data.Runtime.Events.Vision
{
    public readonly struct NoticeUnitVisionToUpdateEvent : IEvent
    {
        public Systems.Unit.Unit Unit { get; }

        public NoticeUnitVisionToUpdateEvent(Systems.Unit.Unit unit)
        {
            Unit = unit;
        }
    }
}

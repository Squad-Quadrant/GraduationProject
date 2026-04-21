using Data.Runtime.Events.Vision;
using Systems.Buff.Config;
using UnityEngine;

namespace Systems.Buff.Event
{
    [CreateAssetMenu(fileName = "NoticeVisionToUpdate", menuName = "Game/Buff/BuffEvent/NoticeVisionToUpdate")]
    public class NoticeVisionToUpdate : UnitBuffEvent
    {
        protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
        {
            buffInfo.EventBus.Publish(new NoticeUnitVisionToUpdateEvent(unit));
        }
    }
}

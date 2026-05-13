using Core.Events;
using Systems.Buff;

namespace Data.Runtime.Events.Buff
{
    public struct BuffTurnEvent : IEvent
    {
        public IBuffAble Buffable;
        public BuffInfo BuffInfo;
        
        public BuffTurnEvent(IBuffAble buffable, BuffInfo buffInfo)
        {
            Buffable = buffable;
            BuffInfo = buffInfo;
        }
    }
}
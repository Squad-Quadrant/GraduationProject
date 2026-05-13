using Core.Events;
using Systems.Buff;

namespace Data.Runtime.Events.Buff
{
    public struct BuffAttachedEvent : IEvent
    {
        public IBuffAble Buffable;
        public BuffInfo BuffInfo;
        
        public BuffAttachedEvent(IBuffAble buffable, BuffInfo buffInfo)
        {
            Buffable = buffable;
            BuffInfo = buffInfo;
        }
    }
}
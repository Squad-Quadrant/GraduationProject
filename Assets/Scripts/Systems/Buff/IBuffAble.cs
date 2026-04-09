using System.Collections.Generic;

namespace Systems.Buff
{
    public interface IBuffAble
    {
        public BuffProxy BuffProxy { get; }

        public BuffInfo GetBuffInfo(int id)
        {
            return BuffProxy.GetBuff(id);
        }

        public void AttachBuff(BuffInfo info)
        {
            BuffProxy.Attach(info);
        }

        public void LostBuff(BuffInfo info)
        {
            BuffProxy.Lost(info);
        }
    }
}
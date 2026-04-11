using Systems.Buff.Config;
using UnityEngine;

namespace Systems.Buff
{
    public interface IBuffService
    {
        public void Register(BuffProxy buffProxy);
        public BuffInfo CreateBuffInfo(BuffType type, IBuffAble target, Object creator);
        // public void AttachBuff(BuffType type, IBuffAble target, Object creator);
        // public void LostBuff(BuffInfo buffInfo);
    }
}
using Systems.Buff.Config;
using UnityEngine;

namespace Systems.Buff
{
    public interface IBuffService
    {
        public void Register(IBuffAble target);
        public BuffInfo CreateBuffInfo(BuffType type, IBuffAble target, object creator);
        // public void AttachBuff(BuffType type, IBuffAble target, Object creator);
        // public void LostBuff(BuffInfo buffInfo);
    }
}
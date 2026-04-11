using Systems.Buff.Config;
using Object = UnityEngine.Object;

namespace Systems.Buff
{
    public interface IBuffAble
    {
        public BuffProxy BuffProxy { get; set; }

        public BuffInfo GetBuffInfo(int id)
        {
            return BuffProxy.GetBuff(id);
        }

        public void AttachBuff(BuffType type, Object creator)
        {
            BuffProxy.Attach(type, creator);
        }

        public void LostBuff(BuffInfo info)
        {
            BuffProxy.Lost(info);
        }
    }
}
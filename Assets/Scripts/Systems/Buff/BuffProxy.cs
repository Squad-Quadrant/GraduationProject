using System;
using System.Collections.Generic;
using System.Linq;

namespace Systems.Buff
{
    public abstract class BuffProxy
    {
        protected List<BuffInfo> buffInfos = new();
        protected readonly IBuffService buffService;
        public List<BuffInfo> BuffInfos => buffInfos;

        protected BuffProxy(IBuffService buffService)
        {
            this.buffService = buffService;
        }
        
        // public virtual void Init()
        // {
        //     foreach (var buffInfo in buffInfos)
        //     {
        //         buffInfo.OnInit();
        //     }
        // }
        
        public virtual void Attach(BuffInfo buffInfo)
        {
            buffInfos.Add(buffInfo);
            buffInfo.OnAttach();
        }
        
        public virtual void Lost(BuffInfo buffInfo)
        {
            buffInfos.Remove(buffInfo);
            buffInfo.OnLost();
        }
        
        public virtual void Turn()
        {
            foreach (var buffInfo in buffInfos)
            {
                buffInfo.OnTurn();
            }
        }

        public virtual void Reset()
        {
            foreach (var buffInfo in buffInfos)
            {
                buffInfo.OnReset();
            }
        }

        public virtual void Property<T>(PropertyType propertyType, ref T baseValue) where T : struct, IConvertible
        {
            foreach (var buffInfo in buffInfos)
            {
                buffInfo.OnProperty(propertyType, ref baseValue);
            }
        }

        public virtual BuffInfo GetBuff(int id)
        {
            return buffInfos.FirstOrDefault(b => b.Id == id);
        }
    }
    
    public class UnitBuffProxy : BuffProxy
    {
        private Unit.Unit _owner;
        
        public UnitBuffProxy(Unit.Unit owner, IBuffService buffService) : base(buffService)
        {
            _owner = owner;
        }
    }
}
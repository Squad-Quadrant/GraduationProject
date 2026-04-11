using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Systems.Buff.Config;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Systems.Buff
{
    [Serializable]
    public class BuffProxy
    {
        [SerializeField]protected List<BuffInfo> buffInfos = new();
        protected readonly IBuffService buffService;
        protected readonly IBuffAble owner;
        public IBuffAble Owner => owner;
        public List<BuffInfo> BuffInfos => buffInfos;

        public BuffProxy(IBuffService buffService, IBuffAble owner)
        {
            this.buffService = buffService;
            this.owner = owner;
            BuffPropertyInjector.Inject(owner);
        }
        
        public virtual void Attach(BuffType buffType, Object creator)
        {
            var sameTypeBuffs = GetBuffs(buffType);
            if (sameTypeBuffs.Count > 0 && sameTypeBuffs[0].Mergeable())
            {
                var theBuff =  sameTypeBuffs[0];
                // creator 的信息会在此处消失，需要注意未来是否有功能依赖
                theBuff.Merge(1);
            }
            else
            {
                var buffInfo = buffService.CreateBuffInfo(buffType, owner, creator);
                if (buffInfo == null)
                {
                    this.LogError($"BuffService.CreateBuffInfo: buffType {buffType} not exist");
                    return;
                }
                buffInfos.Add(buffInfo);
                buffInfo.OnAttach();
            }
        }
        
        public virtual void Lost(BuffInfo buffInfo)
        {
            if (!buffInfos.Contains(buffInfo)) return;
            buffInfos.Remove(buffInfo);
            buffInfo.OnLost();
        }
        
        public virtual void Turn()
        {
            foreach (var buffInfo in buffInfos)
            {
                buffInfo.OnTurn();
            }
            
            List<BuffInfo> lostBuffs = new();
            foreach (var buffInfo in buffInfos)
            {
                if (buffInfo.CurrentStack <= 0)
                {
                    lostBuffs.Add(buffInfo);
                }
            }

            foreach (var buffInfo in lostBuffs)
            {
                Lost(buffInfo);
            }
        }

        public virtual void Reset()
        {
            foreach (var buffInfo in buffInfos)
            {
                buffInfo.OnReset();
            }
        }

        public virtual void ExecutePropertyInfluence(BuffProperty property)
        {
            foreach (var buffInfo in buffInfos)
            {
                buffInfo.OnProperty(property);
            }
        }

        public virtual BuffInfo GetBuff(int uid)
        {
            return buffInfos.FirstOrDefault(b => b.Uid == uid);
        }

        public virtual List<BuffInfo> GetBuffs(BuffType type)
        {
            return buffInfos.Where(b => b.BuffType == type).ToList();
        }
    }
    //
    // public class UnitBuffProxy : BuffProxy
    // {
    //     public Unit.Unit Owner => (Unit.Unit) owner;
    //
    //     public UnitBuffProxy(Unit.Unit owner, IBuffService buffService) : base(buffService, owner)
    //     {
    //         
    //     }
    // }
}
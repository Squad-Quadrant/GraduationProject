using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Damage;
using UnityEngine;

namespace Systems.Damage
{
    public class RecoverExecutingChain : DamageExecutingChain
    {
        public RecoverExecutingChain(DamageExecutingContext context, IEventBus eventBus) : base(context, eventBus)
        { }

        public override DamageType DamageType => DamageType.Recover;
        
        protected override void InitInfluencers()
        {
            
        }

        protected override void InitInfluences()
        {
            // base.InitInfluences();
            int changer = (Context.Info as RecoverTriggeringInfo).Changer;
            influences.Add(new RecoverInfluence(changer, Context.Attacker));
            
            influences.Sort((a, b) => b.Priority - a.Priority);

            foreach (var influence in influences)
            {
                influence.Init(context);
            }
        }

        public override void Execute()
        {
            influences.RemoveAll(i => context.ignoredInfluenceTypes.Any(t => i.DamageInfluenceTypes.Contains(t)));
            
            foreach (var influence in influences)
            {
                influence.Execute();
            }
            
            foreach (var influence in influences)
            {
                influence.Last();
            }
            
            ApplyDamage();
        }

        protected override void ApplyDamage()
        {
            var defender = context.Defender;
            
            int newHp = Mathf.Min(defender.CurrentHp + context.Damage, defender.maxHp);
            int actualHeal = newHp - defender.CurrentHp;
            defender.CurrentHp = newHp;
            this.Log($"{context.Attacker.DisplayName} 为 {defender.DisplayName} 回复血量：{actualHeal}，当前血量：{defender.CurrentHp}/{defender.maxHp}", true);
            eventBus.Publish(new RecoverAppliedEvent(context.GetSnapshot())); // todo:保险起见临时新增一个恢复事件,未来更改DamageService为BloodService,更新所有相关的语义
        }
    }
}
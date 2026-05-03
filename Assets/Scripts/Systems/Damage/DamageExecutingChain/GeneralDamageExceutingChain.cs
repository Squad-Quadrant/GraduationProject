using System.Linq;
using Core.Events;
using Core.Log;
using UnityEngine;

namespace Systems.Damage
{
    public class GeneralDamageExecutingChain : DamageExecutingChain
    {
        public GeneralDamageExecutingChain(DamageExecutingContext context, IEventBus eventBus) : base(context, eventBus)
        {
        }

        public override DamageType DamageType => DamageType.General;
        protected override void InitInfluencers()
        {
            influencers.Add(context.Attacker);
        }

        public override void Execute()
        {
            influences.RemoveAll(i => context.ignoredInfluenceTypes.Any(t => i.DamageInfluenceTypes.Contains(t)));
            
            if (context.IsFinalCalculated)
            {
                context.FinalCalculatedNum = context.CalculateNum;
            }
            else
            {
                for (int i = 0; i < context.CalculateNum; i++)
                {
                    if (context.HitRate > Random.Range(0f, 1f))
                        context.FinalCalculatedNum++;
                }
            }

            for (int i = 0; i < context.FinalCalculatedNum; i++)
            {
                if (context.needResetDamage) 
                    ResetDamage();
                
                context.CurrentDamageIndex = i;
                
                foreach (var influence in influences)
                {
                    influence.Execute();
                }
                ApplyDamage();
            }
            
            if (context.needApplyDamage)
                foreach (var influence in influences)
                {
                    influence.Last();
                }

            if (context.needApplyDamage)
            {
                if(context.isMiss)
                {
                    this.Log($"Attack missed! Defender ID:{context.Defender.id}", true);
                }
                else
                {
                    this.Log($"{context.Attacker.DisplayName}对{context.Defender.DisplayName}造成了{context.TotalDamage}点伤害！", true);
                }
            }
        }
    }
}
using Core.Events;
using Core.Log;
using UnityEngine;

namespace Systems.Damage
{
    public class BulletDamageExecutingChain : DamageExecutingChain
    {
        public BulletDamageExecutingChain(DamageExecutingContext context, IEventBus eventBus) : base(context, eventBus)
        { }

        public override DamageType DamageType => DamageType.Bullet;
        
        protected override void InitInfluencers()
        {
            var theAttacker = context.Attacker as Unit.Unit;
            influencers.Add(theAttacker.CurrentEquipment.Logic as IDamageInfluencer);
            influencers.Add(theAttacker);
        }

        public override void Execute()
        {
            influences.RemoveAll(i => context.ignoredInfluenceTypes.Contains(i.DamageInfluenceType));
            
            for (int i = 0; i < context.CalculateNum; i++)
            {
                if (context.HitRate > Random.Range(0f, 1f)) 
                    context.FinalCalculatedNum++;
            }

            for (int i = 0; i < context.FinalCalculatedNum; i++)
            {
                context.CurrentDamageIndex = i;
                foreach (var influence in influences)
                {
                    influence.Execute();
                }
                if (context.needApplyDamage)
                    ApplyDamage();
                if (context.needResetDamage) 
                    ResetDamage();
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
                }else if (context.DamageType == DamageType.Bullet)
                {
                    var theAttacker = context.Attacker as Unit.Unit;
                    string isOnPreciseShoot = theAttacker.CurrentWeapon.IsOnPreciseShoot ? "精准" : "";
                    this.Log($"{theAttacker.name}使用{theAttacker.CurrentWeapon.Name()}对{context.Defender.name}进行{isOnPreciseShoot}攻击，命中{context.FinalCalculatedNum}发子弹，" +
                             $"击中{context.bodyPartType.ToStr()}, 共造成伤害{context.TotalDamage}，护甲减少{context.TotalDefenseDamage}", true);
                }
            }
        }
    }
}

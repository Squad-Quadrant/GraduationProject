using System;
using Systems.Equipment;
using UnityEngine;

namespace Systems.Damage
{
    public class ShotDefenceDamageInfluence : DamageInfluence
    {
        public ShotDefenceDamageInfluence(IDamageInfluencer owner, int priority = 1) : base(owner, priority)
        {
        }

        public override void Execute()
        {
            var theWeapon = (WeaponLogic)Owner;
            //目前还没有命中部位伤害倍率
            //每次攻击，对护甲造成: 伤害*命中部位伤害倍率*护甲减伤率*护甲承伤率(即100%一武器穿透率)的伤害
            int defenceDamage = Mathf.FloorToInt(theWeapon.GetDamage() * Defender.defenseRate * (1 - theWeapon.PenetrationRate())); 
            
            Context.DefenceDamage += defenceDamage;
        }
    }
}
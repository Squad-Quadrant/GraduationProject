using System.Collections.Generic;
using Systems.Equipment;
using UnityEngine;

namespace Systems.Damage
{
    // 常规射击伤害
    public class ShotDamageInfluence : DamageInfluence
    {
        public ShotDamageInfluence(IDamageInfluencer owner, int priority = 0) : base(owner, priority) { }
        
        public override void Execute()
        {
            var theWeapon = (WeaponLogic)Owner;
            
            // 获得两个单位的距离
            float distance = Vector2Int.Distance(Attacker.position, Defender.position);

            // 伤害衰减
            float damageMultiplier = 1f;

            var damageAttenuation = theWeapon.DamageAttenuation();

            // 每()格衰减()
            int power = Mathf.FloorToInt(distance / damageAttenuation.perGrid);
            damageMultiplier *= Mathf.Pow(damageAttenuation.multiplier, power);
            
            int damage = Mathf.FloorToInt(((WeaponLogic)Owner).GetDamage() * damageMultiplier);
            
            // 对生命值造成
            //目前还没有命中部位伤害倍率
            // 1.若对护甲伤害<护甲当前护甲值，则对生命值造成: 伤害*命中部位伤害倍率*(1-护甲减伤率)*武器穿透率的伤害。
            // 2.若对护甲伤害>护甲当前护甲值，则对生命值造成: 伤害*命中部位伤害倍率*(1-护甲减伤率)*武器穿透率+(对护甲伤害-当前护甲值)的伤害。
            
            if (Context.DefenceDamage > Defender.defense)
            {
                Context.Damage += Mathf.FloorToInt(damage * theWeapon.PenetrationRate() * (1 - Defender.defenseRate) + (Context.DefenceDamage - Defender.defense));
            }
            else
            {
                Context.Damage += Mathf.FloorToInt(damage * theWeapon.PenetrationRate() * (1 - Defender.defenseRate));
            }
        }
    }
}
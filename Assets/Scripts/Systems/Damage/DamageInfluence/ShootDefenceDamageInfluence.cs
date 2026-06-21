using System;
using System.Collections.Generic;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Systems.Damage
{
    public class ShootDefenceDamageInfluence : DamageInfluence
    {
        public Unit.Unit UnitAttacker => Attacker as Unit.Unit;
        public ShootDefenceDamageInfluence(IDamageInfluencer owner, int priority = 1) : base(owner, priority)
        {
        }

        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.Defense };

        public override void Execute()
        {
            var theWeapon = (WeaponLogic)Owner;
            
            // 获得两个单位的距离
            float distance = Mathf.Abs(UnitAttacker.position.x - Defender.position.x) + Mathf.Abs(UnitAttacker.position.y - Defender.position.y);
            
            // 伤害衰减
            float damageMultiplier = 1f;

            var damageAttenuation = theWeapon.DamageAttenuation();

            // 每()格衰减()
            int power = Mathf.FloorToInt(distance / damageAttenuation.perGrid);
            damageMultiplier *= Mathf.Pow(damageAttenuation.multiplier, power);
            
            int damage = Mathf.FloorToInt(((WeaponLogic)Owner).GetDamage() * damageMultiplier);
            
            int defenceDamage = Mathf.FloorToInt(damage * (1 - Defender.defenseRate) * (1 - theWeapon.PenetrationRate())); 
            
            Context.DefenceDamage += defenceDamage;
        }

        public override void Last()
        {

        }
    }
}

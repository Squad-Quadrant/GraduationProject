using System;
using System.Collections.Generic;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Systems.Damage
{
    public class ShootDefenceDamageInfluence : DamageInfluence
    {
        public ShootDefenceDamageInfluence(IDamageInfluencer owner, int priority = 1) : base(owner, priority)
        {
        }

        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.Defense };

        public override void Execute()
        {
            if (Context.bodyPartType != BodyPartType.Head && Context.bodyPartType != BodyPartType.Torso)
                return;
            var theWeapon = (WeaponLogic)Owner;
            int defenceDamage = Mathf.FloorToInt(theWeapon.GetDamage() * (1 - Defender.defenseRate) * (1 - theWeapon.PenetrationRate())); 
            
            Context.DefenceDamage += defenceDamage;
        }

        public override void Last()
        {

        }
    }
}

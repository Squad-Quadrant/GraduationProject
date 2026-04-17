using Systems.Equipment;
using UnityEngine;

namespace Systems.Damage
{
    public class ShotDefenceDamageInfluence : DamageInfluence
    {
        public ShotDefenceDamageInfluence(IDamageInfluencer owner, int priority = 1) : base(owner, priority)
        {
        }

        public override DamageInfluenceType DamageInfluenceType => DamageInfluenceType.Defence;

        public override void Execute()
        {
            var theWeapon = (WeaponLogic)Owner;
            int defenceDamage = Mathf.FloorToInt(theWeapon.GetDamage() * Defender.defenseRate * (1 - theWeapon.PenetrationRate())); 
            
            Context.DefenceDamage += defenceDamage;
        }

        public override void Last()
        {
            
        }
    }
}
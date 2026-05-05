using System.Collections.Generic;
using Systems.Damage;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment.Logic
{
	public class WeaponLogic : EquipmentLogic, IDamageInfluencer
    {
	    protected int Ammo;
	    public bool IsOnPreciseShoot = false;
        public string DisplayName => Name();
        public bool FullAmmo => Ammo >= AmmoCapacity();
        public WeaponConfig WeaponConfig { get; }

        public WeaponLogic(WeaponConfig config, Unit owner) : base(config, owner)
        {
	        WeaponConfig = config;
	        Ammo = config.ammoCapacity;
        }

        public virtual bool CanPreciseShoot() => WeaponConfig.canPreciseShoot;

        public virtual int CurrentAmmo(int delta = 0)
        {
	        if (delta == 0) return Ammo;

	        Ammo += delta;
            Ammo = Mathf.Clamp(Ammo, 0, AmmoCapacity());
            Owner.TriggerInfoChanged();
            return Ammo;
        }

        public virtual int ShootSpeed() => WeaponConfig.shootSpeed;

        public virtual int AmmoCapacity() => WeaponConfig.ammoCapacity;

        public virtual int PreciseShootSpeed() => WeaponConfig.preciseShootSpeed;

        public virtual float PreciseShootHitRateBonus() => WeaponConfig.preciseShootHitRateBonus;

        public virtual List<ShotRange> ShotRange() => WeaponConfig.shotRanges;

        public virtual DamageAttenuation DamageAttenuation() => WeaponConfig.damageAttenuation;

        public virtual float PenetrationRate() => WeaponConfig.penetrationRate;

        public override int GetDamage()
        {
            return WeaponConfig.damage;
        }

        public override int Range() => int.MaxValue;

        public override bool CheckAttackable(Unit target)
        {
            if (target == null || target.faction == Owner.faction)
                return false;
            return Vector2Int.Distance(Owner.position, target.position) <= Range() && CurrentAmmo() > 0;
        }


        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            if (context.DamageType == DamageType.Bullet)
            {
                return new List<DamageInfluence>{
                    new ShootDamageInfluence(this, 0, IsOnPreciseShoot),
                    new ShootHitRateInfluence(this, 0, IsOnPreciseShoot),
                    new ShootDefenceDamageInfluence(this),
                    new BodyDestructionInfluence(this, IsOnPreciseShoot)
                };
            }
            return null;
        }
    }
}

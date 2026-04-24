using System.Collections.Generic;
using Systems.Damage;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment.Logic
{
	public class WeaponLogic : EquipmentLogic, IDamageInfluencer
    {
	    private readonly WeaponConfig _weaponConfig;

	    protected int Ammo;
	    public bool IsOnPreciseShoot = false;
        public string DisplayName => Name();
        public bool FullAmmo => Ammo >= AmmoCapacity();

        public WeaponLogic(WeaponConfig config, Unit owner) : base(config, owner)
        {
	        _weaponConfig = config;
	        Ammo = config.ammoCapacity;
        }

        public virtual bool CanPreciseShoot() => _weaponConfig.canPreciseShoot;

        public virtual int CurrentAmmo(int delta = 0)
        {
	        if (delta == 0) return Ammo;

	        Ammo += delta;
            Ammo = Mathf.Clamp(Ammo, 0, AmmoCapacity());
            Owner.TriggerInfoChanged();
            return Ammo;
        }

        public virtual int ShootSpeed() => _weaponConfig.shootSpeed;

        public virtual int AmmoCapacity() => _weaponConfig.ammoCapacity;

        public virtual int PreciseShootSpeed() => _weaponConfig.preciseShootSpeed;

        public virtual float PreciseShootHitRateBonus() => _weaponConfig.preciseShootHitRateBonus;

        public virtual List<ShotRange> ShotRange() => _weaponConfig.shotRanges;

        public virtual DamageAttenuation DamageAttenuation() => _weaponConfig.damageAttenuation;

        public virtual float PenetrationRate() => _weaponConfig.penetrationRate;

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

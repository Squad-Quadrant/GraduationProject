using System.Collections.Generic;
using Systems.Damage;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment
{
    // 装备系统：
    // 单位可装备不同的各式武器装备，包括一把主武器、一把副武器和三个战术道具。主武器大多为枪械。战术道具包括：投掷类道具、医疗类道具、侦查类道具等。
    // 拆分 EquipmentConfig 之后，基类 Logic 只持有基类 Config（用于访问 Name/Damage 等共用字段）
    // 子类 Logic 额外持有自己的强类型 Config 字段（_weaponConfig / _tacticalItemConfig）
    public abstract class EquipmentLogic
    {
        // Logic不暴露Config,所有属性的获取都需要用Logic做转向获取
        protected readonly EquipmentConfig Config;
        public Unit Owner { get; private set; }

        protected EquipmentLogic(EquipmentConfig config, Unit owner)
        {
            Owner = owner;
            Config = config;
        }

        public virtual string Name() => Config.nName;

        public virtual int GetDamage() => Config.damage;

        public abstract int Range();

        public abstract bool CheckAttackable(Unit target);
    }

    public class WeaponLogic : EquipmentLogic, IDamageInfluencer
    {
	    private readonly WeaponConfig _weaponConfig;

	    protected int Ammo;
	    public bool IsOnPreciseShoot = false;

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
                    new ShotHitRateInfluence(this, 0, IsOnPreciseShoot),
                    new ShotDefenceDamageInfluence(this),
                    new BodyDestructionInfluence(this)
                };
            }
            return null;
        }
    }

    public class TacticalItemLogic : EquipmentLogic, IDamageInfluencer
    {
	    private readonly TacticalItemConfig _tacticalItemConfig;

	    public TacticalItemLogic(TacticalItemConfig config, Unit owner) : base(config, owner) => _tacticalItemConfig = config;

	    public override int Range() => _tacticalItemConfig.attackRange;

	    public override bool CheckAttackable(Unit target)
	    {
		    if (target == null || target.faction == Owner.faction)
			    return false;
		    return Vector2Int.Distance(Owner.position, target.position) <= Range();
	    }

	    public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context) => null;
    }
}

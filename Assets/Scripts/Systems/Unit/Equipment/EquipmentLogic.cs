using System.Collections.Generic;
using Systems.Damage;
using Systems.Equipment.Config;
using UnityEngine;

namespace Systems.Equipment
{
    // 装备系统：
    // 单位可装备不同的各式武器装备，包括一把主武器、一把副武器和三个战术道具。主武器大多为枪械。战术道具包括：投掷类道具、医疗类道具、侦查类道具等。
    
    public abstract class EquipmentLogic
    {
        // Logic不暴露Config,所有属性的获取都需要用Logic做转向获取
        protected readonly EquipmentConfig Config;
        public Unit.Unit Owner { get; private set; }

        public EquipmentLogic(EquipmentConfig config, Unit.Unit owner)
        {
            Owner = owner;
            Config = config;
        }
       
        public virtual int GetDamage()
        {
            return Config.Damage;
        }

        public abstract int Range();
    }
    
    public class WeaponLogic : EquipmentLogic, IDamageInfluencer
    {
        protected int currentAmmo;
        public WeaponLogic(EquipmentConfig config, Unit.Unit owner) : base(config, owner)
        {
            currentAmmo = config.AmmoCapacity;
        }

        public virtual bool CanPreciseShoot()
        {
            return Config.canPreciseShoot;
        }
        
        public virtual int CurrentAmmo(int delta = 0)
        {
            currentAmmo += delta;
            currentAmmo = Mathf.Clamp(currentAmmo, 0, AmmoCapacity());
            if (delta != 0)
                Owner.TriggerInfoChanged();
            return currentAmmo;
        }

        public virtual int ShootSpeed()
        {
            return Config.shootSpeed;
        }

        public virtual int AmmoCapacity()
        {
            return Config.AmmoCapacity;
        }
        
        public virtual List<ShotRange> ShotRange()
        {
            return Config.ShotRanges;
        }
        
        public virtual DamageAttenuation DamageAttenuation()
        {
            return Config.DamageAttenuation;
        }

        public virtual float PenetrationRate()
        {
            return Config.PenetrationRate;
        }
        
        public override int Range()
        {
            return int.MaxValue;
        }

        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            if (context.DamageType == DamageType.Bullet)
            {
                return new List<DamageInfluence>{
                    new ShotDamageInfluence(this), 
                    new ShotHitRateInfluence(this), 
                    new ShotDefenceDamageInfluence(this) };
            }

            return null;
        }
    }
    
    public class TacticalItemLogic : EquipmentLogic, IDamageInfluencer
    {
        public TacticalItemLogic(EquipmentConfig config, Unit.Unit owner) : base(config, owner)
        {
        }

        public override int Range()
        {
            return Config.AttackRange;
        }

        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            return null;
        }
    }
}
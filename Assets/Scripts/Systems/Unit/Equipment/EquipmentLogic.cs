using System.Collections.Generic;
using Systems.Damage;
using Systems.Equipment.Config;

namespace Systems.Equipment
{
    // 装备系统：
    // 单位可装备不同的各式武器装备，包括一把主武器、一把副武器和三个战术道具。主武器大多为枪械。战术道具包括：投掷类道具、医疗类道具、侦查类道具等。
    
    // 我不是很想封装地狱,先这样写着吧
    public abstract class EquipmentLogic
    {
        protected EquipmentConfig Config;
        public EquipmentLogic(EquipmentConfig config) => Config = config;
       
        public virtual int GetDamage()
        {
            return Config.Damage;
        }

        // 获取攻击范围
        public abstract int GetRange();
    }
    
    public class WeaponLogic : EquipmentLogic, IDamageInfluencer
    {
        public WeaponLogic(EquipmentConfig config) : base(config)
        {
        }

        public override int GetRange()
        {
            return int.MaxValue;
        }

        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            if (context.DamageType == DamageType.Bullet)
            {
                return new List<DamageInfluence>{new ShotDamageInfluence(this) };
            }

            return null;
        }
    }
    
    public class TacticalItemLogic : EquipmentLogic, IDamageInfluencer
    {
        public TacticalItemLogic(EquipmentConfig config) : base(config)
        {
        }

        public override int GetRange()
        {
            return Config.AttackRange;
        }

        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            return null;
        }
    }
}
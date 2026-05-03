using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment.Logic
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

        public abstract int GetDamage();
        public virtual Sprite Icon() => Config.icon;
        public virtual string Description() => Config.description;

        public abstract int Range();

        public abstract bool CheckAttackable(Unit target);
    }
}

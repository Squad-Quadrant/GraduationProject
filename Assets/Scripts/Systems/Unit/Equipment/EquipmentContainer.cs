using System;
using Systems.Unit.Equipment.Config;

namespace Systems.Unit.Equipment
{
	// 装备槽位容器
	// 类型身份即分派依据，不再需要枚举字段
    public class EquipmentContainer
    {
	    public EquipmentConfig Config { get; private set; }
	    public EquipmentLogic Logic { get; private set; }

        public Unit Owner { get; private set; }

        public void Init(EquipmentConfig config, Unit owner)
        {
            Owner = owner;
            Config = config;
            if (!config) return;
            Logic = config switch
            {
	            WeaponConfig weaponConfig => new WeaponLogic(weaponConfig, Owner),
	            TacticalItemConfig tacticalItemConfig => new TacticalItemLogic(tacticalItemConfig, Owner),
	            _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}

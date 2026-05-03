using System;
using Systems.Unit.Equipment.Config;
using Systems.Unit.Equipment.Logic;

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
	            TacticalItemConfig tacticalItemConfig => CreateTacticalItemLogic(tacticalItemConfig, Owner),
	            _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static TacticalItemLogic CreateTacticalItemLogic(TacticalItemConfig config, Unit owner) =>
	        config.kind switch
	        {
		        ETacticalItemKind.InstantMedpack => new InstantMedpackLogic(config, owner),
		        ETacticalItemKind.Grenade        => new ThrowableGrenadeLogic(config, owner),
		        ETacticalItemKind.Burn           => new ThrowableBurnLogic(config, owner),
		        ETacticalItemKind.TimerBomb      => new ThrowableTimerBombLogic(config, owner),
		        ETacticalItemKind.ScoutEye       => new ThrowableScoutEyeLogic(config, owner),
		        ETacticalItemKind.Light       => new ThrowableLightLogic(config, owner),
		        ETacticalItemKind.Smoke       => new ThrowableSmokeLogic(config, owner),
		        _ => throw new ArgumentOutOfRangeException(nameof(config.kind), config.kind, $"Unknown TacticalItemKind: {config.kind}"),
	        };
    }
}

using PurpleFlowerCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Unit.Equipment.Config
{
	// 战术道具配置
	// todo: 战术道具系统的数值配置需要进一步完善
	[Configurable("Equipment/TacticalItem")]
	[CreateAssetMenu(fileName = "TacticalItemConfig", menuName = "Game/Equipment/TacticalItem", order = 1)]
	public class TacticalItemConfig : EquipmentConfig
	{
		[LabelText("手雷类武器的攻击范围")]
		public int attackRange;
	}
}

using PurpleFlowerCore;
using Sirenix.OdinInspector;
using Systems.Buff.Config;
using UnityEngine;

namespace Systems.Unit.Equipment.Config
{
	// 战术道具配置
	[Configurable("Equipment/TacticalItem")]
	[CreateAssetMenu(fileName = "TacticalItemConfig", menuName = "Game/Unit/Equipment/TacticalItem", order = 1)]
	public class TacticalItemConfig : EquipmentConfig
	{
		[Title("Tactical Item")]
		[LabelText("道具种类")]
		public ETacticalItemKind kind = ETacticalItemKind.InstantMedpack;

		[LabelText("每场战斗最大使用次数"), MinValue(1)]
		public int maxUsesPerBattle = 1;

		[LabelText("使用/投掷消耗 AP"), MinValue(0)]
		public int apCost = 1;

		private bool IsThrowable => kind != ETacticalItemKind.InstantMedpack;

		[Title("Throwable"), ShowIf(nameof(IsThrowable))]
		[LabelText("投掷最大距离（曼哈顿）"), MinValue(1)]
		public int throwRange = 5;

		// 覆盖格形状：相对 TargetCell 的偏移
		[Title("Throwable"), ShowIf(nameof(IsThrowable))]
		[LabelText("覆盖格相对偏移")]
		[Tooltip("相对 TargetCell（落点）的偏移，决定覆盖格形状。必须包含 (0,0) 才能包括落点本身。")]
		public Vector2Int[] coverageOffsets = { Vector2Int.zero };

		[Title("Medpack"), ShowIf(nameof(kind), ETacticalItemKind.InstantMedpack)]
		[LabelText("治疗量"), MinValue(1)]
		public int healAmount = 30;

		[Title("Grenade"), ShowIf(nameof(kind), ETacticalItemKind.Grenade)]
		[LabelText("直接伤害"), MinValue(0)]
		public int directDamage = 50;
        
        private bool CanAttachBuff =>
            kind is ETacticalItemKind.Burn or ETacticalItemKind.Grenade;

		[Title("Burn"), ShowIf(nameof(CanAttachBuff))]
		[LabelText("附加 Buff 类型")]
		public BuffType appliedBuff;

		private bool HasPersistTurns =>
			kind is ETacticalItemKind.Burn or ETacticalItemKind.TimerBomb or ETacticalItemKind.ScoutEye or ETacticalItemKind.Light or ETacticalItemKind.Smoke;

		[Title("AreaEffect Lifetime"), ShowIf(nameof(HasPersistTurns))]
		[LabelText("持续回合数"), MinValue(1)]
		public int persistTurns = 2;

		[Title("AreaEffect Visual")]
		[LabelText("OneShot特效")]
		public GameObject oneShotVfxPrefab;

		[ShowIf(nameof(HasPersistTurns))]
		[LabelText("持续区域特效")]
		public GameObject persistentVfxPrefab;

		[Title("Throwable"), ShowIf(nameof(IsThrowable))]
        [LabelText("投掷物 Prefab")]
        [Tooltip("飞行中的投掷物预制体")]
        public GameObject projectilePrefab;

		[Title("ScoutEye"), ShowIf(nameof(kind), ETacticalItemKind.ScoutEye)]
		[LabelText("视野透视半径"), MinValue(1)]
		public int visionReach = 5;
	}
}

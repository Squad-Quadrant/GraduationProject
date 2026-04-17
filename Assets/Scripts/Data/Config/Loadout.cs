using System;
using Sirenix.OdinInspector;

namespace Data.Config
{
	// 单位运行时装备装配方案
	// 玩家局外UI中配置后进行装配，之后传入局内，在局内通过DataManager查id得到对应的EquipmentConfig进行实例化
	[Serializable]
	public class Loadout
	{
		public const int TacticalItemSlotCount = 3;

		[LabelText("主武器ID")]
		public int mainWeaponId;

		[LabelText("副武器ID")]
		public int secondaryWeaponId;

		[LabelText("战术道具ID (固定3个槽位)")]
		public int[] tacticalItemIds = new int[TacticalItemSlotCount];

		// 由 DataManager 在消费 Loadout 前调用，强制规范化战术道具槽位长度，避免因配置失误导致的运行时错误
		public void NormalizeTacticalSlots()
		{
			if (tacticalItemIds is { Length: TacticalItemSlotCount }) return;

			var normalized = new int[TacticalItemSlotCount];
			if (tacticalItemIds != null)
			{
				int copyLen = Math.Min(tacticalItemIds.Length, TacticalItemSlotCount);
				Array.Copy(tacticalItemIds, normalized, copyLen);
			}
			tacticalItemIds = normalized;
		}
	}
}

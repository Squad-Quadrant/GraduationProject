using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Systems.Unit.Equipment
{
	public static class WeaponWeight
	{
		public const int MinStatAfterPenalty = 1;

		public static (int speed, int moveRange) SumPenalty(EquipmentConfig main, EquipmentConfig secondary)
		{
			int speed = 0, moveRange = 0;
			Accumulate(main, ref speed, ref moveRange);
			Accumulate(secondary, ref speed, ref moveRange);
			return (speed, moveRange);
		}

		public static int ApplyPenalty(int baseValue, int penalty) => Mathf.Max(MinStatAfterPenalty, baseValue - penalty);

		private static void Accumulate(EquipmentConfig config, ref int speed, ref int moveRange)
		{
			if (config is not WeaponConfig weapon) return;
			speed += weapon.weightSpeed;
			moveRange += weapon.weightMoveRange;
		}
	}
}

using System;
using System.Collections.Generic;
using PurpleFlowerCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Unit.Equipment.Config
{
	// 射击类武器配置
	[Configurable("Equipment/Weapon")]
	[CreateAssetMenu(fileName = "WeaponConfig", menuName = "Game/Unit/Equipment/Weapon", order = 0)]
	public class WeaponConfig : EquipmentConfig
	{
        [LabelText("伤害")] public int damage;
        [LabelText("精神伤害")] public int mentalDamage;
        
		[LabelText("射击类武器的弹容量")]
		public int ammoCapacity;

		[LabelText("射击类武器的攻击范围和对应的命中率")]
		public List<ShotRange> shotRanges;

		[LabelText("伤害衰减")]
		public DamageAttenuation damageAttenuation;

		[LabelText("穿透率")]
		public float penetrationRate;

		[LabelText("射速 (发/点AP)")]
		public int shootSpeed;

		[LabelText("是否可以精确射击")]
		public bool canPreciseShoot;

		[LabelText("精确射击模式下的射速 (发/点AP)")]
		public int preciseShootSpeed;

		[LabelText("精确射击模式下的命中率加成")]
		public float preciseShootHitRateBonus;
	}

	[Serializable]
	public struct ShotRange
	{
		public int min;
		public float hitRate;
	}

	[Serializable]
	public struct DamageAttenuation
	{
		public int perGrid;
		public float multiplier;
	}
}

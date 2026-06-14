using System;
using System.Collections.Generic;
using Presentation.Unit;
using PurpleFlowerCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Unit.Equipment.Config
{
	public enum WeaponType
	{
		Normal,
		Grapeshot
	}
	
	// 射击类武器配置
	[Configurable("Equipment/Weapon")]
	[CreateAssetMenu(fileName = "WeaponConfig", menuName = "Game/Unit/Equipment/Weapon", order = 0)]
	public class WeaponConfig : EquipmentConfig
	{
		[LabelText("Spine动画使用的名称")] public string spineName;

		[LabelText("握持方式")] public EGripType gripType;

        [LabelText("伤害")]
        public int damage;

        [LabelText("精神伤害")]
        public int mentalDamage;
        
        [LabelText("枪械类型")]
        public WeaponType weaponType;
        
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
        
        [LabelText("重量-SP")] public int weightSpeed;
        
        [LabelText("重量-MP")] public int weightMoveRange;

		[Title("Animation")]
		[LabelText("动画键(可空)")]
		public string animKey;

		[Title("Audio")]
		[LabelText("开火音效")] public AudioClip fireClip;
		[LabelText("换弹音效")] public AudioClip reloadClip;
		[LabelText("空仓音效")] public AudioClip emptyClip;
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

using System;
using System.Collections.Generic;
using PurpleFlowerCore;
using UnityEngine;

namespace Systems.Equipment.Config
{
    // 武器装备相关数值：
    // · 伤害：使用该武器攻击时能造成的基础伤害。
    // · 精神伤害：使用特殊类别武器时会对SAN值造成的伤害，大部分普通武器精神伤害为0。
    // · 伤害范围：使用部分特殊类别武器或战术道具时，造成伤害的范围大小。即对该范围内的单元格上的所有目标造成伤害。
    // · 噪音范围：使用武器进行攻击等行动会产生的声音的范围。消音类武器攻击时不会发出声音。
    // · 重量（便携程度）：单位装备武器时，会减少单位的速度/移动力。重量影响该值的大小。
    // · 弹容量：枪械类武器的弹药容量。当武器弹匣内弹药不足时，需要进行换弹。
    // · 命中率：单位使用武器攻击时，该武器的当前“命中率”会影响本次攻击是否命中。与被攻击单位之间的距离、是否存在掩体等均会影响命中率。
    // 命中率减益机制（初步设想）：
    // 根据被攻击单位所处单元格的状态和是否存在掩体进行计算，若存在多种状态，可累加。
    // 掩体：命中率－20%
    // 战争迷雾：命中率－40%
    // 昏暗：命中率－15%
    // · 伤害衰减：单位使用该武器攻击时，与被攻击单位之间的距离会影响本次攻击的伤害。在霰弹枪类武器上尤为明显。
    // · 射程：单位使用该武器攻击时，与被攻击单位之间的距离会影响本次攻击的命中率。
    // · 穿透率：武器造成伤害时，穿透护甲造成伤害的能力。
    // · 射速：武器的射速，代表单位使用该武器进行攻击时，每消耗一点AP所能造成伤害的次数（发射子弹的数量）。
    // · 精确射击模式：部分自动武器在攻击时可以转换射击模式为精确射击模式。精确射击模式将降低射速，但是大幅度提高命中率。
    [Configurable("Equipment")]
    [CreateAssetMenu(fileName = "EquipmentConfig", menuName = "Configs/EquipmentConfig", order = 0)]

    public class EquipmentConfig : ScriptableObject
    {
        [Tooltip("唯一ID")]
        public int Id;
        
        [Tooltip("名称")]
        public string Name;
        
        [Tooltip("描述")]
        public string Description;
        
        [Tooltip("图标")]
        public Sprite Icon;
        
        [Tooltip("伤害")]
        public int Damage;

        [Tooltip("精神伤害")]
        public int MentalDamage;
        
        [Tooltip("重量")]
        public float Weight;
        
        [Tooltip("武器类型-可精确到具体武器，也可以是更抽象的类别，只要逻辑可以统一")]
        public EquipmentType Type;

        #region Weapon-specific properties
        
        [Tooltip("射击类武器的弹容量")]
        public int AmmoCapacity;
        
        [Tooltip("射击类武器的攻击范围和对应的命中率")]
        public List<ShotRange> ShotRanges;
        
        [Tooltip("伤害衰减")]
        public DamageAttenuation DamageAttenuation;

        [Tooltip("穿透率")] 
        public float PenetrationRate;

        #endregion

        #region Equipment-specific properties
        
        [Tooltip("手雷类武器的攻击范围")]
        public int AttackRange;
        
        #endregion

    }

    public enum EquipmentType
    {
        [InspectorName("请选择装备类型")]
        None = 0,
        [InspectorName("常规射击武器")]
        Weapon,
        [InspectorName("常规手雷")]
        TacticalItem,
        [InspectorName("投掷物发射器")]
        GrenadeLauncher,
        [InspectorName("请选择装备类型")]
        Count
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

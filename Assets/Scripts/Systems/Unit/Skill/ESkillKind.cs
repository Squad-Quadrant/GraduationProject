using UnityEngine;

namespace Systems.Unit.Skill
{
    // 不区分主动技能和被动技能，在配置中决定
	public enum ESkillKind
	{
        None = 0,
        // -------------------------
        [InspectorName("区域检测")] AreaReconnaissance,
        // [InspectorName("侦查装备")] ReconnaissanceEquipment,
        [InspectorName("战术翻滚")] TacticalRoll,
        // [InspectorName("精准打击")] PrecisionShot,
        [InspectorName("斗志昂扬")] FightMorale,
        // [InspectorName("屏息凝神")] DeepConcentration,
        [InspectorName("守护")] Guard,
        // [InspectorName("铜墙铁壁")] IronWall,
        [InspectorName("冲锋陷阵")] ChargeForward,
        // [InspectorName("防爆盾")] Shield,
        // -------------------------
        Count,
	}
}

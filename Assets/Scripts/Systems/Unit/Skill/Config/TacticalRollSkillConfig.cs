using PurpleFlowerCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Unit.Skill
{
    [Configurable("Skill/TacticalRoll")]
	[CreateAssetMenu(fileName = "TacticalRollSkillConfig", menuName = "Game/Unit/Skill/TacticalRoll")]
    public class TacticalRollSkillConfig : SkillConfig
    {
        [LabelText("翻滚距离"), MinValue(1)]
        public int distance = 5;

        [LabelText("动画速度倍率"), MinValue(0.01f)]
        public float animationSpeedMultiplier = 2.5f;
    }
}
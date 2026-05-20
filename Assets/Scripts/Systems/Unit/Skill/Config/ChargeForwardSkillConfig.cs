using PurpleFlowerCore;
using Systems.Buff.Config;
using UnityEngine;

namespace Systems.Unit.Skill
{
    [Configurable("Skill/ChargeForward")]
	[CreateAssetMenu(fileName = "ChargeForwardSkillConfig", menuName = "Game/Unit/Skill/ChargeForward")]
    public class ChargeForwardSkillConfig : SkillConfig
    {
        public BuffType toAddType;
    }
}
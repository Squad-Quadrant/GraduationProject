using PurpleFlowerCore;
using UnityEngine;

namespace Systems.Unit.Skill
{
    [Configurable("Skill/FightMorale")]
	[CreateAssetMenu(fileName = "FightMoraleSkillConfig", menuName = "Game/Unit/Skill/FightMorale")]
    public class FightMoraleSkillConfig : SkillConfig
    {
        public int apRecover = 2;
    }
}
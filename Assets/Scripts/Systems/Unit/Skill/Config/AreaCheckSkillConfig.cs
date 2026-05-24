using PurpleFlowerCore;
using UnityEngine;

namespace Systems.Unit.Skill
{
    [Configurable("Skill/AreaCheck")]
	[CreateAssetMenu(fileName = "AreaCheckSkillConfig", menuName = "Game/Unit/Skill/AreaCheck")]
    public class AreaCheckSkillConfig : AddAreaSkillConfig
    {
        public float hitRateChanger = 0.15f;
    }
}
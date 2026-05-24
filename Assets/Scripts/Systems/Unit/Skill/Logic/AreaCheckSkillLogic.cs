using Systems.AreaEffect.Behaviors;
using Systems.Interaction;

namespace Systems.Unit.Skill.Logic
{
    public class AreaCheckSkillLogic : AddAreaSkillLogic
    {
        public AreaCheckSkillLogic(SkillConfig config, Unit owner) : base(config, owner)
        {
        }

        public override void Use(InteractionContext ctx)
        {
            BuildAreaEffect(Owner.position, new AreaCheckBehavior(Config as AreaCheckSkillConfig, AddAreaConfig.skillName, AddAreaConfig.icon, AddAreaConfig.persistentVfxPrefab), ctx);
        }
    }
}
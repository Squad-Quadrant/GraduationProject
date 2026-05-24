using Core.Events;
using Systems.Unit.Skill.Logic;

namespace Data.Runtime.Events.Skill
{
    public struct SkillUsedEvent : IEvent
    {
        public Systems.Unit.Unit Owner;
        public SkillLogic Logic;
        
        public SkillUsedEvent(Systems.Unit.Unit owner, SkillLogic logic)
        {
            Owner = owner;
            Logic = logic;
        }
    }
}
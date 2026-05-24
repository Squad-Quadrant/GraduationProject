using System;

namespace Systems.Unit.Skill.Logic
{
	public static class SkillLogicFactory
	{
		public static SkillLogic Create(SkillConfig config, Unit owner)
		{
			if (!config || owner == null) return null;

            switch (config.kind)
            {
                case ESkillKind.FightMorale:
                    return new FightMoraleSkillLogic(config, owner);
                case ESkillKind.ChargeForward:
	                return new ChargeForwardSkillLogic(config, owner);
                case ESkillKind.AreaCheck:
                    return new AreaCheckSkillLogic(config, owner);
                case ESkillKind.TacticalRoll:
                case ESkillKind.Guard:
                case ESkillKind.Count:
                case ESkillKind.None:
                default:
                    throw new NotSupportedException(
                        $"Unknown ESkillKind: {config.kind}. Add a case to SkillLogicFactory.Create.");
            }
		}
	}
}

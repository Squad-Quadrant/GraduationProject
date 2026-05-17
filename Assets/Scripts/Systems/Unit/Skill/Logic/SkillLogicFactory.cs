using System;

namespace Systems.Unit.Skill.Logic
{
	public static class SkillLogicFactory
	{
		public static SkillLogic Create(SkillConfig config, Unit owner)
		{
			if (!config || owner == null) return null;

			// return config.kind switch
			// {
			// 	// ESkillKind.AreaReconnaissance => new InstantHealLogic(config, owner),
			// 	// ESkillKind.ScoutEye    => new ScoutEyeSkillLogic(config, owner),
			// 	_ => throw new NotSupportedException(
			// 		$"Unknown ESkillKind: {config.kind}. Add a case to SkillLogicFactory.Create."),
			// };
            return null;
        }
	}
}

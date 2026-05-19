using System;
using Core.Log;

namespace Systems.Unit.Skill.Logic
{
	public class FightMoraleSkillLogic : InstantlyUseSkillLogic
	{
		public FightMoraleSkillLogic(SkillConfig config, Unit owner) : base(config, owner)
		{
            
		}

        public override void Use()
        {
            FightMoraleSkillConfig config = Config as FightMoraleSkillConfig;

            int newAp = Owner.CurrentAp + config.apRecover;
            newAp = newAp > Owner.maxAp ? Owner.maxAp : newAp;
            int actualRecover = newAp - Owner.CurrentAp;
            Owner.CurrentAp += actualRecover;

            Owner.CanAttack.Value = true;
            
            this.Log($"{Owner.name}使用{Name}, {Description}", true);
        }
    }
}

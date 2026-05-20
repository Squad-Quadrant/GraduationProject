using System;
using Core.Log;
using Systems.Buff;

namespace Systems.Unit.Skill.Logic
{
	public class ChargeForwardSkillLogic : InstantlyUseSkillLogic
	{
		public ChargeForwardSkillLogic(SkillConfig config, Unit owner) : base(config, owner)
		{
			
		}

        public override void Use()
        {
	        var confit = Config as ChargeForwardSkillConfig;
	        (Owner as IBuffAble).AttachBuff(confit.toAddType, this);
        }
    }
}

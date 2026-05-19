using System;

namespace Systems.Unit.Skill.Logic
{
	public abstract class SkillLogic
	{
		public SkillConfig Config { get; }
		public Unit Owner { get; }

		private int _currentCooldown;
		public int CurrentCooldown => _currentCooldown;

		public virtual bool CanUse => _currentCooldown == 0 && Owner.CurrentAp >= Config.apCost;

        public virtual string Name => Config.skillName;
        
        public virtual int ApCost => Config.apCost;
        
        public virtual string Description => Config.description;

		protected SkillLogic(SkillConfig config, Unit owner)
		{
			Config = config;
			Owner = owner;
		}

		public void Consume()
		{
			_currentCooldown = Config.cooldown;
			Owner.TriggerInfoChanged();
		}

		public void OnOwnerTurnStart()
		{
			if (_currentCooldown <= 0) return;

			_currentCooldown--;
			Owner.TriggerInfoChanged();
		}
        
	}
}

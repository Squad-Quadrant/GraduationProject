using System.Collections.Generic;
using Data.Runtime;

namespace Systems.Damage
{
    public class DamageExecutingChain
    {
        public DamageType DamageType;
        private DamageExecutingContext _context;
        private readonly List<DamageInfluence> _influences = new();
        private readonly List<IDamageInfluencer> _influencers = new();
        
        public DamageExecutingChain(DamageType damageType)
        {
            DamageType = damageType;
        }

        public void Init(DamageExecutingContext context)
        {
            context.Owner = this;
            _context = context;
            
            if (_context.DamageType == DamageType.Bullet)
            {
                _influencers.Add(_context.Attacker.GetEquipment(_context.ActionType).Logic as IDamageInfluencer);
            }

            InitInfluences();
        }

        private void InitInfluences()
        {
            foreach (var influencer in _influencers)
            {
                _influences.AddRange(influencer.GetDamageInfluences(_context));
            }
            _influences.Sort((a, b) => b.Priority - a.Priority);

            foreach (var influence in _influences)
            {
                influence.Init(_context);
            }
        }

        public void Execute()
        {
            foreach (var influence in _influences)
            {
                influence.Execute();
            }

            ApplyDamage();
        }

        private void ApplyDamage()
        {
            var defender = _context.Defender;
            defender.currentHp -= _context.Damage;
        }
    }

    public class DamageExecutingContext
    {
        public Unit.Unit Attacker;
        public Unit.Unit Defender;
        public EActionType ActionType;
        public DamageType DamageType;

        public DamageExecutingChain Owner;

        public DamageExecutingContext(Unit.Unit attacker, Unit.Unit defender, EActionType actionType,  
            DamageExecutingChain owner)
        {
            Attacker = attacker;
            Defender = defender;
            ActionType = actionType;
            Owner = owner;
            DamageType = owner.DamageType;
        }

        public float HitRate;
        public int Damage;
        public int DefenceDamage; // 对护甲的伤害
        public int MentalDamage; // San值伤害
    }
}
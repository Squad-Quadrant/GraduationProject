using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Damage;
using UnityEngine;

namespace Systems.Damage
{
    public class DamageExecutingChain
    {
        public DamageType DamageType;
        private DamageExecutingContext _context;
        private readonly List<DamageInfluence> _influences = new();
        private readonly List<IDamageInfluencer> _influencers = new();
        private IEventBus _eventBus;
        
        public DamageExecutingChain(DamageType damageType)
        {
            DamageType = damageType;
        }

        public void Init(DamageExecutingContext context, IEventBus eventBus)
        {
            _eventBus = eventBus;
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
            _context.isMiss = _context.HitRate < Random.Range(0f, 1f);
            if (!_context.isMiss)
            {
                defender.defense -= _context.DefenceDamage;
                // defender.curr -= _context.MentalDamage;
                defender.currentHp -= _context.Damage;
                this.Log($"Damage applied: {_context.Damage} damage, {_context.DefenceDamage} defense damage," +
                         $" {_context.MentalDamage} mental damage. Defender ID:{defender.id} HP: {defender.currentHp}, Defense: {defender.defense}");
            }
            else
            {
                this.Log($"Attack missed! Defender ID:{defender.id}");
            }
            _eventBus.Publish(new DamageAppliedEvent(_context));
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

        public float HitRate = 1;
        public int Damage = 0;
        public int DefenceDamage = 0; // 对护甲的伤害
        public int MentalDamage = 0; // San值伤害
        public bool isMiss;
    }
}
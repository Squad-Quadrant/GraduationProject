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
            for (int i = 0; i < _context.CalculateNum; i++)
            {
                if (_context.HitRate > Random.Range(0f, 1f)) 
                    _context.FinalCalculatedNum++;
            }

            for (int i = 0; i < _context.FinalCalculatedNum; i++)
            {
                _context.CurrentDamageIndex = i;
                foreach (var influence in _influences)
                {
                    influence.Execute();
                }
                if (_context.needApplyDamage)
                    ApplyDamage();
                if (_context.needResetDamage) 
                    ResetDamage();
            }

            if (_context.needApplyDamage)
            {
                if(_context.isMiss)
                {
                    this.Log($"Attack missed! Defender ID:{_context.Defender.id}", true);
                }else if (_context.DamageType == DamageType.Bullet)
                {
                    string isOnPreciseShoot = _context.Attacker.CurrentWeapon.isOnPreciseShoot ? "精准" : "";
                    this.Log($"{_context.Attacker.name}使用{_context.Attacker.CurrentWeapon.Name()}对{_context.Defender.name}进行{isOnPreciseShoot}攻击，命中{_context.FinalCalculatedNum}发子弹，" +
                             $"共造成伤害{_context.TotalDamage}，护甲减少{_context.TotalDefenceDamage}", true);
                }
            }
        }

        private void ApplyDamage()
        {
            var defender = _context.Defender;

            defender.CurrentDefense -= _context.DefenceDamage;
            defender.CurrentSan -= _context.SanDamage;
            defender.CurrentHp -= _context.Damage;
            
            _context.TotalDamage += _context.Damage;
            _context.TotalDefenceDamage += _context.DefenceDamage;
            _context.TotalMentalDamage += _context.SanDamage;
            
            this.Log($"Damage applied: type:{_context.DamageType}, {_context.Damage} damage, {_context.DefenceDamage} defense damage," +
                $" {_context.SanDamage} mental damage. Defender ID:{defender.id} HP: {defender.CurrentHp}, Defense: {defender.CurrentDefense}");

            _eventBus.Publish(new DamageAppliedEvent(_context.GetSnapshot()));
        }

        private void ResetDamage()
        {
            _context.Damage = 0;
            _context.DefenceDamage = 0;
            _context.SanDamage = 0;
        }
    }

    public record DamageExecutingContext
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

        public int Damage = 0;
        public int DefenceDamage = 0; // 对护甲的伤害
        public int SanDamage = 0; // San值伤害
        
        public float HitRate = 1;
        
        public int CalculateNum = 1; // 该伤害流程的重复计算次数
        public int FinalCalculatedNum = 0; // 添加命中率影响后的实际应用数量
        public int CurrentDamageIndex = 0; // 当前正在计算的伤害序号（用于多次伤害的情况）
        public bool isMiss => FinalCalculatedNum == 0;
        public bool needApplyDamage = true;
        public bool needResetDamage = true; // 在每次重新计算伤害之前是否需要重置伤害

        public int TotalDamage = 0;
        public int TotalDefenceDamage = 0;
        public int TotalMentalDamage = 0;


        public DamageExecutingContext GetSnapshot()
        {
            return this with { };
        }
    }
}
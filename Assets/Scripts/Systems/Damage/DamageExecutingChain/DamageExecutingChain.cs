using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Damage;
using UnityEngine;

namespace Systems.Damage
{
    public abstract class DamageExecutingChain
    {
        public abstract DamageType DamageType { get; }
        protected DamageExecutingContext context;
        public DamageExecutingContext Context => context;
        protected List<DamageInfluence> influences = new();
        protected List<IDamageInfluencer> influencers = new();
        protected IEventBus eventBus;
        
        public DamageExecutingChain(DamageExecutingContext context, IEventBus eventBus)
        {
            this.eventBus = eventBus;
            this.context = context;
            context.Owner = this;
        }
        
        public void Init()
        {
            InitInfluencers();
            InitInfluences();
        }

        protected abstract void InitInfluencers();

        protected virtual void InitInfluences()
        {
            foreach (var influencer in influencers)
            {
                influences.AddRange(influencer.GetDamageInfluences(context));
            }
            influences.Sort((a, b) => b.Priority - a.Priority);

            foreach (var influence in influences)
            {
                influence.Init(context);
            }
        }

        public abstract void Execute();

        protected virtual void ApplyDamage()
        {
            var defender = context.Defender;

            int finalDamage = Mathf.RoundToInt(context.Damage * context.DamageModifier);
            int finalDefenseDamage = Mathf.RoundToInt(context.DefenceDamage * context.DefenseDamageModifier);
            int finalSanDamage = Mathf.RoundToInt(context.SanDamage * context.SanDamageModifier);

            defender.CurrentHp -= finalDamage;
            defender.CurrentDefense -= finalDefenseDamage;
            defender.CurrentSan -= finalSanDamage;

            defender.BodyPartInfo[context.bodyPartType] += finalDamage;

            context.TotalDamage += finalDamage;
            context.TotalDefenseDamage += finalDefenseDamage;
            context.TotalSanDamage += finalSanDamage;
            
            this.Log($"Damage applied: type:{context.DamageType}, {context.Damage} damage, {context.DefenceDamage} defense damage," +
                $" {context.SanDamage} mental damage. Defender ID:{defender.id} HP: {defender.CurrentHp}, Defense: {defender.CurrentDefense}");

            eventBus.Publish(new DamageAppliedEvent(context.GetSnapshot()));
        }

        protected virtual void ResetDamage()
        {
            context.Damage = 0;
            context.DefenceDamage = 0;
            context.SanDamage = 0;
        }
    }
    
    public record DamageExecutingContext
    {
        public IDamageInfluencer Attacker;
        public Unit.Unit Defender;
        public EActionType ActionType;
        public DamageType DamageType => Owner.DamageType;

        public DamageExecutingChain Owner;
        public DamageExecutingContext(IDamageInfluencer attacker, Unit.Unit defender)
        {
            Attacker = attacker;
            Defender = defender;
        }
        
        public int Damage = 0;
        public int DefenceDamage = 0; // 对护甲的伤害
        public int SanDamage = 0; // San值伤害
        
        public float DamageModifier = 1;
        public float DefenseDamageModifier = 1;
        public float SanDamageModifier = 1;

        public BodyPartType bodyPartType; // 击中部位
        public List<DamageInfluenceType> ignoredInfluenceTypes = new(); 
        
        public float HitRate = 1;
        
        public int CalculateNum = 1; // 该伤害流程的重复计算次数
        public int FinalCalculatedNum = 0; // 添加命中率影响后的实际应用数量
        public int CurrentDamageIndex = 0; // 当前正在计算的伤害序号（用于多次伤害的情况）
        public bool IsFinalCalculated => CurrentDamageIndex == FinalCalculatedNum - 1; // 是否是最后一次伤害计算
        
        public bool isMiss => FinalCalculatedNum == 0;
        public bool needApplyDamage = true;
        public bool needResetDamage = true; // 在每次重新计算伤害之前是否需要重置伤害

        public int TotalDamage = 0;
        public int TotalDefenseDamage = 0;
        public int TotalSanDamage = 0;


        public DamageExecutingContext GetSnapshot()
        {
            return this with { };
        }
    }
}
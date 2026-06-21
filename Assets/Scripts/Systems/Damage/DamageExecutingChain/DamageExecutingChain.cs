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
                var toAddInfluences = influencer.GetDamageInfluences(context);
                influences.AddRange(toAddInfluences);
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
            
            context.FinalDamage[context.CurrentDamageIndex] = finalDamage;
            context.FinalDefenseDamage[context.CurrentDamageIndex] = finalDefenseDamage;
            context.FinalSanDamage[context.CurrentDamageIndex] = finalSanDamage;

            context.TotalDamage += finalDamage;
            context.TotalDefenseDamage += finalDefenseDamage;
            context.TotalSanDamage += finalSanDamage;
            
            if (context.needApplyDamage)
            {
                defender.CurrentHp -= finalDamage;
                defender.CurrentDefense -= finalDefenseDamage;
                defender.CurrentSan -= finalSanDamage;

                defender.BodyPartInfo[context.bodyPartType] += finalDamage;

                this.Log(
                    $"Damage applied: type:{context.DamageType}, {context.Damage} damage, {context.DefenceDamage} defense damage," +
                    $" {context.SanDamage} mental damage. Defender ID:{defender.id} HP: {defender.CurrentHp}, Defense: {defender.CurrentDefense}");

                eventBus.Publish(new DamageAppliedEvent(context.GetSnapshot()));
            }
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
        public DamageTriggeringInfo Info;
        public DamageExecutingContext(DamageTriggeringInfo info)
        {
            Attacker = info.Attacker;
            Defender = info.Defender;
            Info = info;
        }
        
        public int Damage = 0;
        public int DefenceDamage = 0; // 对护甲的伤害
        public int SanDamage = 0; // San值伤害
        
        public float DamageModifier = 1;
        public float DefenseDamageModifier = 1;
        public float SanDamageModifier = 1;

        public bool UseDefense = true; // 是否计算护甲影响

        public BodyPartType bodyPartType = BodyPartType.None; // 击中部位
        public List<DamageInfluenceType> ignoredInfluenceTypes = new();
        
        public float HitRate = 1;
        public List<(string, string)> HitRateInfluences = new(); // 用来存对命中率有影响的因素，方便展示记录
        
        public int CalculateNum = 1; // 该伤害流程的重复计算次数
        public int FinalCalculatedNum = 0; // 添加命中率影响后的实际应用数量
        public int CurrentDamageIndex = 0; // 当前正在计算的伤害序号（用于多次伤害的情况）
        public bool IsFinalCalculated => CurrentDamageIndex == FinalCalculatedNum - 1; // 是否是最后一次伤害计算
        
        public bool isMiss => FinalCalculatedNum == 0;
        public bool needApplyDamage = true;
        public bool needResetDamage = true; // 在每次重新计算伤害之前是否需要重置伤害

        public Dictionary<int, int> FinalDamage = new();
        public Dictionary<int, int> FinalDefenseDamage = new();
        public Dictionary<int, int> FinalSanDamage = new();

        public int TotalDamage = 0;
        public int TotalDefenseDamage = 0;
        public int TotalSanDamage = 0;

        public bool IsSimulating;

        public void AddHitRateInfluence(string reason, float changer = 0, float multiplier = 1)
        {
            HitRate += changer;
            HitRate *= multiplier;
            string value = "";
            if (changer != 0) 
                value += $"{(changer > 0 ? "+" : "")}{changer * 100}%";
            if (!Mathf.Approximately(multiplier, 1) || changer == 0) 
                value += $"*{multiplier * 100}%";
            HitRateInfluences.Add((reason, value));
        }

        public DamageExecutingContext GetSnapshot()
        {
            return this with { };
        }
    }
}
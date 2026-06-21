using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Systems.Buff;
using Systems.Buff.Config;
using Random = UnityEngine.Random;

namespace Systems.Damage
{
    public enum BodyPartType
    {
        None,
        Legs,
        Arms,
        Head,
        Torso
    }

    public struct BodyPartHitInfo
    {
        public BodyPartType PartType;
        // public float HitProbability;
        public bool HasArmor;
        public float DamageMultiplier;
    }

    public static class BodyDestructionConst
    {
        public static Dictionary<BodyPartType, float> Rate = new()
        {
            { BodyPartType.Legs, 0.15f },
            { BodyPartType.Arms, 0.13f },
            { BodyPartType.Head, 0.08f },
            { BodyPartType.Torso, 0.64f },
        };

        public static Dictionary<BodyPartType, float> PreciseRate = new()
        {
            { BodyPartType.Legs, 0.1f },
            { BodyPartType.Arms, 0.1f },
            { BodyPartType.Head, 0.3f },
            { BodyPartType.Torso, 0.5f },
        };
    }

    public class BodyDestructionInfluence : DamageInfluence
    {
        protected BodyPartHitInfo hitPart;
        public bool _isOnPreciseShoot;
        private static readonly BodyPartHitInfo[] BodyParts = 
        {
            new() { PartType = BodyPartType.Legs, HasArmor = false, DamageMultiplier = 0.6f },
            new() { PartType = BodyPartType.Arms, HasArmor = false, DamageMultiplier = 0.65f },
            new() { PartType = BodyPartType.Head, HasArmor = true, DamageMultiplier = 2f },
            new() { PartType = BodyPartType.Torso, HasArmor = true, DamageMultiplier = 1f }
        };

        public BodyDestructionInfluence(IDamageInfluencer owner, bool isOnPreciseShoot = false, int priority = 2) :
            base(owner, priority)
        {
            _isOnPreciseShoot = isOnPreciseShoot;
        }


        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.BodyDestruction };

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);

            if (Context.IsSimulating && context.bodyPartType != BodyPartType.None)
            {
                hitPart = BodyParts.First(p => p.PartType == context.bodyPartType);
            }
            else
            {
                hitPart = GetRandomBodyPart();
            }
            
            Context.DamageModifier = hitPart.DamageMultiplier;
            Context.bodyPartType = hitPart.PartType;

            if (hitPart.HasArmor && Context.Defender.CurrentDefense > 0)
            {
                Context.DefenseDamageModifier = hitPart.DamageMultiplier;
            }
            else
            {
                Context.UseDefense = false;
                Context.ignoredInfluenceTypes.Add(DamageInfluenceType.Defense);
            }
        }

        public override void Execute()
        {
            
        }

        public override void Last()
        {
            // 如果累计伤害超过50，施加效果
            int currentPartDamage = Context.Defender.BodyPartInfo[hitPart.PartType];
            int beforePartDamage = Context.Defender.BodyPartInfo[hitPart.PartType] - Context.TotalDamage;

            if (beforePartDamage < 1 && currentPartDamage >= 1)
            {
                switch (hitPart.PartType)
                {
                    case BodyPartType.Legs:
                        (Context.Defender as IBuffAble).AttachBuff(BuffType.LegFracture, Owner);
                        this.Log($"对{Defender.name}的造成腿部骨折", true);
                        break;
                    case BodyPartType.Arms:
                        (Context.Defender as IBuffAble).AttachBuff(BuffType.HandFracture, Owner);
                        this.Log($"对{Defender.name}的造成手部骨折", true);
                        break;
                    case BodyPartType.Head:
                        (Context.Defender as IBuffAble).AttachBuff(BuffType.Dizzy, Owner);
                        this.Log($"对{Defender.name}的造成眩晕", true);
                        break;
                    case BodyPartType.Torso:
                        (Context.Defender as IBuffAble).AttachBuff(BuffType.Bleed, Owner);
                        this.Log($"对{Defender.name}的造成流血", true);
                        break;
                }
            }
        }

        private BodyPartHitInfo GetRandomBodyPart()
        {
            var rateDic = _isOnPreciseShoot ? BodyDestructionConst.PreciseRate : BodyDestructionConst.Rate;
            float totalWeight = 0;
            foreach (var part in BodyParts) totalWeight += rateDic[part.PartType];

            float randomValue = Random.Range(0, totalWeight);
            float currentWeight = 0;

            foreach (var part in BodyParts)
            {
                currentWeight += rateDic[part.PartType];
                if (randomValue <= currentWeight)
                {
                    return part;
                }
            }

            return BodyParts[BodyParts.Length - 1];
        }
    }
    
    public static class BodyDestructionExtensions
    {
        public static string ToStr(this BodyPartType bodyPartType)
        {
            switch (bodyPartType)
            {
                case BodyPartType.Legs:
                    return "腿部";
                case BodyPartType.Arms:
                    return "手臂";
                case BodyPartType.Head:
                    return "头部";
                case BodyPartType.Torso:
                    return "躯干";
                case BodyPartType.None:
                    return "无";
                default:
                    throw new ArgumentOutOfRangeException(nameof(bodyPartType), bodyPartType, null);
            }
        }
    }
}
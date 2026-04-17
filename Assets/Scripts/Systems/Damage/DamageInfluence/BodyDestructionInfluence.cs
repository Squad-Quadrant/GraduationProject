using System;
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
        public float HitProbability;
        public bool HasArmor;
        public float DamageMultiplier;
    }

    public class BodyDestructionInfluence : DamageInfluence
    {
        protected BodyPartHitInfo hitPart;
        private static readonly BodyPartHitInfo[] BodyParts = 
        {
            new() { PartType = BodyPartType.Legs, HitProbability = 20f, HasArmor = false, DamageMultiplier = 0.6f },
            new() { PartType = BodyPartType.Arms, HitProbability = 100000f, HasArmor = false, DamageMultiplier = 0.65f },
            new() { PartType = BodyPartType.Head, HitProbability = 10f, HasArmor = true, DamageMultiplier = 2f },
            new() { PartType = BodyPartType.Torso, HitProbability = 50f, HasArmor = true, DamageMultiplier = 1f }
        };

        public BodyDestructionInfluence(IDamageInfluencer owner, int priority = 2) : base(owner, priority)
        { }


        public override DamageInfluenceType DamageInfluenceType => DamageInfluenceType.BodyDestruction;

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);
            
            hitPart = GetRandomBodyPart();
            
            Context.DamageModifier = hitPart.DamageMultiplier;
            Context.bodyPartType = hitPart.PartType;

            if (hitPart.HasArmor)
            {
                Context.DefenseDamageModifier = hitPart.DamageMultiplier;
            }
            else
            {
                Context.ignoredInfluenceTypes.Add(DamageInfluenceType.Defence);
            }
        }

        public override void Execute()
        {
            
        }

        public override void Last()
        {
            // 如果累计伤害超过50，施加效果
            int currentPartDamage = Context.Defender.bodyPartInfo[hitPart.PartType];
            int beforePartDamage = Context.Defender.bodyPartInfo[hitPart.PartType] - Context.TotalDamage;

            if (beforePartDamage < 30 && currentPartDamage >= 30)
            {
                switch (hitPart.PartType)
                {
                    case BodyPartType.Legs:
                        // 骨折判定等
                        break;
                    case BodyPartType.Arms:
                        (Context.Defender as IBuffAble).AttachBuff(BuffType.Fracture, Owner);
                        this.Log($"对{Defender.name}的造成骨折", true);
                        break;
                    case BodyPartType.Head:
                        // 眩晕判定等
                        break;
                    case BodyPartType.Torso:
                        // 流血判定等
                        break;
                }
            }
        }

        private BodyPartHitInfo GetRandomBodyPart()
        {
            float totalWeight = 0;
            foreach (var part in BodyParts) totalWeight += part.HitProbability;

            float randomValue = Random.Range(0, totalWeight);
            float currentWeight = 0;

            foreach (var part in BodyParts)
            {
                currentWeight += part.HitProbability;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(bodyPartType), bodyPartType, null);
            }
        }
    }
}
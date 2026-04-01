using Data.Runtime;

namespace Systems.Damage
{
    public abstract class DamageInfluence
    {
        public IDamageInfluencer Owner;
        public Unit.Unit Attacker;
        public Unit.Unit Defender;
        public EActionType ActionType;
        public int Priority = 0; // 优先级，数值越大优先执行, 可以考虑先用枚举做粗略分级,再用整数做细分
        public DamageExecutingContext Context; 
        public DamageInfluence(IDamageInfluencer owner, int priority = 0)
        {
            Owner = owner;
            Priority = priority;
        }

        public virtual void Init(DamageExecutingContext context)
        {
            Context = context;
            Attacker = context.Attacker;
            Defender = context.Defender;
            ActionType = context.ActionType;
        }
        
        public abstract void Execute();
    }
}
using Systems.Equipment;

namespace Systems.Damage
{
    // 常规射击伤害
    public class ShotDamageInfluence : DamageInfluence
    {
        public ShotDamageInfluence(IDamageInfluencer owner, int priority = 0) : base(owner, priority) { }
        
        public override void Execute()
        {
            Context.Damage += ((WeaponLogic)Owner).GetDamage();
        }
    }
}
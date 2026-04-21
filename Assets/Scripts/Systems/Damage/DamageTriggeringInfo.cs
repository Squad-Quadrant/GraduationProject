using Data.Runtime;

namespace Systems.Damage
{
    public abstract class DamageTriggeringInfo
    {
        public object Attacker;
        public Unit.Unit Defender;
        public abstract DamageType DamageType { get; }
    }
    
    public class BulletDamageTriggeringInfo : DamageTriggeringInfo
    {
        public Unit.Unit UnitAttacker => Attacker as Unit.Unit;
        public EActionType ActionType;
        public BulletDamageTriggeringInfo( Unit.Unit attacker, Unit.Unit defender, EActionType actionType)
        {
            Attacker = attacker;
            Defender = defender;
            ActionType = actionType;
        }

        public override DamageType DamageType => DamageType.Bullet;
    }

    public class GeneralDamageTriggeringInfo : DamageTriggeringInfo
    {
        public IDamageInfluencer Attacker;
        public Unit.Unit Defender;

        public GeneralDamageTriggeringInfo(IDamageInfluencer attacker, Unit.Unit defender)
        {
            Attacker = attacker;
            Defender = defender;
        }

        public override DamageType DamageType => DamageType.General;
    }
}
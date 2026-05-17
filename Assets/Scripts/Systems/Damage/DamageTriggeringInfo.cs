using System;
using System.Collections.Generic;
using Data.Runtime;
using Systems.Buff.Influence;

namespace Systems.Damage
{
    public abstract class DamageTriggeringInfo
    {
        public IDamageInfluencer Attacker;
        public Unit.Unit Defender;
        public abstract DamageType DamageType { get; }
    }
    
    public class BulletDamageTriggeringInfo : DamageTriggeringInfo
    {
        public Unit.Unit UnitAttacker => Attacker as Unit.Unit;
        public EActionType ActionType;
        public List<IDamageInfluencer> Environment;
        public BulletDamageTriggeringInfo( Unit.Unit attacker, Unit.Unit defender, EActionType actionType, 
            List<IDamageInfluencer> environment = null)
        {
            Attacker = attacker;
            Defender = defender;
            ActionType = actionType;
            Environment = environment; 
        }

        public override DamageType DamageType => DamageType.Bullet;
    }

    public class GeneralDamageTriggeringInfo : DamageTriggeringInfo
    {
        public GeneralDamageTriggeringInfo(IDamageInfluencer attacker, Unit.Unit defender)
        {
            Attacker = attacker;
            Defender = defender;
        }

        public override DamageType DamageType => DamageType.General;
    }
    
    public class RecoverTriggeringInfo : DamageTriggeringInfo
    {
        public int Changer;
        public RecoverTriggeringInfo(IDamageInfluencer attacker, Unit.Unit defender, int changer)
        {
            Changer = changer;
            Attacker = attacker;
            Defender = defender;
        }

        public override DamageType DamageType => DamageType.Recover;
    }
}
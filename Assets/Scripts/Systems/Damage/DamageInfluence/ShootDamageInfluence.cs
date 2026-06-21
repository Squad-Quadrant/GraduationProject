using System.Collections.Generic;
using Systems.Unit.Equipment.Logic;
using UnityEngine;

namespace Systems.Damage
{
    // 常规射击伤害
    public class ShootDamageInfluence : DamageInfluence
    {
        public Unit.Unit UnitAttacker => Attacker as Unit.Unit;
        private bool _isOnPreciseShoot;
        public ShootDamageInfluence(IDamageInfluencer owner, int priority = 0, bool isOnPreciseShoot = false) : base(
            owner, priority)
        {
            _isOnPreciseShoot = isOnPreciseShoot;
        }

        public override List<DamageInfluenceType> DamageInfluenceTypes => new() { DamageInfluenceType.Damage };

        public override void Init(DamageExecutingContext context)
        {
            base.Init(context);
            var theWeapon = (WeaponLogic)Owner;

                if (!_isOnPreciseShoot)
                {
                    int bulletNum = theWeapon.CurrentAmmo() > theWeapon.ShootSpeed() ? theWeapon.ShootSpeed() : theWeapon.CurrentAmmo();
                    if (context.needApplyDamage)
                       theWeapon.CurrentAmmo(-bulletNum);
                    Context.CalculateNum = bulletNum;
                }
                else
                {
                    int bulletNum = theWeapon.CurrentAmmo() > theWeapon.PreciseShootSpeed() ? theWeapon.PreciseShootSpeed() : theWeapon.CurrentAmmo();
                    if (context.needApplyDamage)
                        theWeapon.CurrentAmmo(-bulletNum);
                    Context.CalculateNum = bulletNum;
                }
        }

        public override void Execute()
        {
            var theWeapon = (WeaponLogic)Owner;
            
            // 获得两个单位的距离
            float distance = Mathf.Abs(UnitAttacker.position.x - Defender.position.x) + Mathf.Abs(UnitAttacker.position.y - Defender.position.y);

            // 伤害衰减
            float damageMultiplier = 1f;

            var damageAttenuation = theWeapon.DamageAttenuation();

            // 每()格衰减()
            int power = Mathf.FloorToInt(distance / damageAttenuation.perGrid);
            damageMultiplier *= Mathf.Pow(damageAttenuation.multiplier, power);
            
            int damage = Mathf.FloorToInt(((WeaponLogic)Owner).GetDamage() * damageMultiplier);
            
            // 对生命值造成
            //目前还没有命中部位伤害倍率
            // 1.若对护甲伤害<护甲当前护甲值，则对生命值造成: 伤害*命中部位伤害倍率*(1-护甲减伤率)*武器穿透率的伤害。
            // 2.若对护甲伤害>护甲当前护甲值，则对生命值造成: 伤害*命中部位伤害倍率*(1-护甲减伤率)*武器穿透率+(对护甲伤害-当前护甲值)的伤害。

            float defenceMultiplier = 1 - Defender.defenseRate;
            
            if (Context.UseDefense)
            {
                if (Context.DefenceDamage > Defender.maxDefense)
                {
                    Context.Damage += Mathf.FloorToInt(damage * theWeapon.PenetrationRate() * defenceMultiplier +
                                                       (Context.DefenceDamage - Defender.CurrentDefense));
                }
                else
                {
                    Context.Damage += Mathf.FloorToInt(damage * theWeapon.PenetrationRate() * defenceMultiplier);
                }
            }
            else
            {
                Context.Damage = Mathf.FloorToInt(damage);
            }
        }

        public override void Last()
        {
            
        }
    }
}
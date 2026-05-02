using Core.Log;
using Sirenix.Utilities;
using Systems.Buff;
using Systems.Buff.Config;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class AttachBuffAreaBehavior : AreaEffectBehavior
	{
		private readonly BuffType _buffType;

		public AttachBuffAreaBehavior(
			BuffType buffType,
			string displayName,
			Sprite displayIcon,
			GameObject persistentVfxPrefab = null)
			: base(displayName, displayIcon, persistentVfxPrefab)
		{
			_buffType = buffType;
		}

        public override void OnUnitEntered(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx)
		{
            TryAddBuff(unit);
		}

        public override void OnUnitLeft(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx)
        {
            TryRemoveBuff(unit);
        }

		public override void OnUnitTurnStart(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx)
		{
            
		}

        private void TryAddBuff(Unit.Unit unit)
        {
            IBuffAble buffAble = unit;
            if (buffAble.BuffProxy.GetBuffs(_buffType).IsNullOrEmpty())
            {
                buffAble.AttachBuff(_buffType, this);
            }
        }
        
        private void TryRemoveBuff(Unit.Unit unit)
        {
            IBuffAble buffAble = unit;
            buffAble.LostBuffs(_buffType);
        }
	}
}

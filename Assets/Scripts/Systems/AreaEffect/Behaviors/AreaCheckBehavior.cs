using System.Linq;
using Systems.Damage;
using Systems.Unit;
using Systems.Unit.Skill;
using Systems.Vision;
using Unity.VisualScripting;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class AreaCheckBehavior : AreaEffectBehavior
	{
		private RevealToken _token = RevealToken.Invalid;
        private AreaCheckSkillConfig _config;
        private GeneralHitRateInfluence _influence;

		public AreaCheckBehavior(
            AreaCheckSkillConfig config,
			string displayName,
			Sprite displayIcon,
			GameObject persistentVfxPrefab = null)
			: base(displayName, displayIcon, persistentVfxPrefab, destroyOnOwnerDeath: true)
        {
            _config = config;
            _influence = new GeneralHitRateInfluence(1, config.hitRateChanger, "区域检测", null);
        }
		public override void OnCreated(AreaEffect self, AreaEffectContext ctx)
		{
			_token = ctx.VisionService.AddTemporaryReveal(Cells.ToList());
		}

		public override void OnRemoved(AreaEffect self, AreaEffectContext ctx)
		{
			if (!_token.IsValid) return;
			ctx.VisionService.RemoveTemporaryReveal(_token);
			_token = RevealToken.Invalid;
		}

        public override void OnUnitEntered(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx)
        {
            base.OnUnitEntered(self, unit, cell, ctx);
            if (unit.faction != EUnitFaction.Player)
            {
                unit.BeHitDamageInfluences.Add(_influence);
            }
        }

        public override void OnUnitLeft(AreaEffect self, Unit.Unit unit, Vector2Int cell, AreaEffectContext ctx)
        {
            base.OnUnitLeft(self, unit, cell, ctx);
            if (unit.faction != EUnitFaction.Player)
            {
                unit.BeHitDamageInfluences.Remove(_influence);
            }
        }
    }
}

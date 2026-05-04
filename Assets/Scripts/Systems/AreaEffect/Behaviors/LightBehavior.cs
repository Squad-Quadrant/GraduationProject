using System.Linq;
using Systems.Vision;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class LightBehavior : AreaEffectBehavior
	{
		private RevealToken _token = RevealToken.Invalid;

		public LightBehavior(
			string displayName,
			Sprite displayIcon,
			GameObject persistentVfxPrefab = null)
			: base(displayName, displayIcon, persistentVfxPrefab, destroyOnOwnerDeath: true)
		{
		}
		public override void OnCreated(AreaEffect self, AreaEffectContext ctx)
		{
			// var visibleCells = ctx.VisionCalculator.CalculateVisibleCells(self.TargetCell, 1);
			_token = ctx.VisionService.AddTemporaryReveal(Cells.ToList());
		}

		public override void OnRemoved(AreaEffect self, AreaEffectContext ctx)
		{
			if (!_token.IsValid) return;
			ctx.VisionService.RemoveTemporaryReveal(_token);
			_token = RevealToken.Invalid;
		}
	}
}

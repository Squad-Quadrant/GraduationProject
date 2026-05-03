using System.Linq;
using Systems.Vision;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class SmokeBehavior : AreaEffectBehavior
	{
		private readonly int _visionRadius;

		private RevealToken _token = RevealToken.Invalid;

		public SmokeBehavior(
			int visionRadius,
			string displayName,
			Sprite displayIcon,
			GameObject persistentVfxPrefab = null)
			: base(displayName, displayIcon, persistentVfxPrefab, destroyOnOwnerDeath: true)
		{
			_visionRadius = visionRadius;
		}
		public override void OnCreated(AreaEffect self, AreaEffectContext ctx)
		{
			var visibleCells = ctx.VisionCalculator.CalculateVisibleCells(self.TargetCell, _visionRadius);
			_token = ctx.VisionService.AddTemporaryReveal(visibleCells.ToList());
		}

		public override void OnRemoved(AreaEffect self, AreaEffectContext ctx)
		{
			if (!_token.IsValid) return;
			ctx.VisionService.RemoveTemporaryReveal(_token);
			_token = RevealToken.Invalid;
		}
	}
}

using System.Linq;
using Systems.Vision;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class ScoutEyeBehavior : AreaEffectBehavior
	{
		private readonly int _visionRadius;

		public override string DisplayName { get; }
		public override Sprite DisplayIcon { get; }
		public override bool DestroyOnOwnerDeath => true;

		private RevealToken _token = RevealToken.Invalid;

		public ScoutEyeBehavior(int visionRadius, string displayName, Sprite displayIcon)
		{
			_visionRadius = visionRadius;
			DisplayName = displayName;
			DisplayIcon = displayIcon;
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

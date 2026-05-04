using System.Linq;
using Systems.Vision;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class SmokeBehavior : AreaEffectBehavior
	{
		private ObscureToken _token = ObscureToken.Invalid;

		public SmokeBehavior(
			string displayName,
			Sprite displayIcon,
			GameObject persistentVfxPrefab = null)
			: base(displayName, displayIcon, persistentVfxPrefab, destroyOnOwnerDeath: true)
		{
		}
		public override void OnCreated(AreaEffect self, AreaEffectContext ctx)
		{
			// var visibleCells = ctx.VisionCalculator.CalculateVisibleCells(self.TargetCell, 1);
			_token = ctx.VisionService.AddTemporaryObscure(Cells.ToList());
            ctx.AIService.AddObscuresCells(Cells.ToList());
		}

		public override void OnRemoved(AreaEffect self, AreaEffectContext ctx)
		{
			if (!_token.IsValid) return;
			ctx.VisionService.RemoveTemporaryObscure(_token);
			_token = ObscureToken.Invalid;
            ctx.AIService.RemoveObscuresCells(Cells.ToList());
		}
	}
}

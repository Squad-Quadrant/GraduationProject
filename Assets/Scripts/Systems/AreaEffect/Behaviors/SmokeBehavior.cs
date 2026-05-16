using System.Linq;
using Systems.Vision;
using UnityEngine;

namespace Systems.AreaEffect.Behaviors
{
	public class SmokeBehavior : AreaEffectBehavior
	{
		private VisionBlockerToken _token = VisionBlockerToken.Invalid;

		public SmokeBehavior(
			string displayName,
			Sprite displayIcon,
			GameObject persistentVfxPrefab = null)
			: base(displayName, displayIcon, persistentVfxPrefab, destroyOnOwnerDeath: true)
		{
		}
		public override void OnCreated(AreaEffect self, AreaEffectContext ctx)
		{
			_token = ctx.VisionService.AddVisionBlocker(Cells.ToList());
            ctx.AIService.AddObscuresCells(Cells.ToList());
		}

		public override void OnRemoved(AreaEffect self, AreaEffectContext ctx)
		{
			if (!_token.IsValid) return;
			ctx.VisionService.RemoveVisionBlocker(_token);
			_token = VisionBlockerToken.Invalid;
            ctx.AIService.RemoveObscuresCells(Cells.ToList());
		}
	}
}

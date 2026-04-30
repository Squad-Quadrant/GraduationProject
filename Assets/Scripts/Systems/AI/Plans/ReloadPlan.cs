using System.Collections.Generic;
using Core.Log;
using Systems.AI.Actions;

namespace Systems.AI.Plans
{
	public class ReloadPlan : ITurnPlan
	{
		public string Name => "Reload";

		public bool IsViable(AIContext context)
		{
			var unit = context.Self;
			var weaponLogic = unit.CurrentWeaponLogic;

			return weaponLogic is { FullAmmo: false } && unit.HasAp;
		}

		public float Score(AIContext context)
		{
			var profile = context.Archetype?.reloadProfile;
			if (profile == null)
			{
				this.LogWarning($"'{context.Self.id}' has no reloadProfile, returning 0");
				return 0f;
			}

			var weaponLogic = context.Self.CurrentWeaponLogic;
			float ammoRatio = (float)weaponLogic.CurrentAmmo() / weaponLogic.AmmoCapacity();
			float lowness = 1f - ammoRatio;
			float ammoFactor = profile.ammoLowAxis.Evaluate(lowness);

			return profile.baseScore * ammoFactor;
		}

		public Queue<IAtomicAction> BuildActionSequence(AIContext context)
		{
			var queue = new Queue<IAtomicAction>();
			queue.Enqueue(new ReloadAction());
			return queue;
		}

		public bool ShouldAbort(AIContext context) => false;
	}
}

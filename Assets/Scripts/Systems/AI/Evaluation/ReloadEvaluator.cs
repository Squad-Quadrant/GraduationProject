using System.Collections.Generic;
using Core.Log;
using Data.Runtime;

namespace Systems.AI.Evaluation
{
	public class ReloadEvaluator : IActionEvaluator
	{
		private const float DefaultBaseScore = 0.3f;
        private const float AmmoAnxiety = 1f;
        private const float Tactics = 0.1f;

		public List<AIActionOption> Evaluate(AIContext context)
		{
			var results = new List<AIActionOption>();
			var unit = context.Self;
			var brain = context.Brain;
            var currentWeapon = unit.CurrentWeapon;
            
            if (currentWeapon == null) return results;

			float baseScore = brain ? brain.reloadBase : DefaultBaseScore;
            float tactics = brain ? brain.tacticsReload : Tactics;
            float ammoAnxiety = brain ? brain.ammoAnxiety : AmmoAnxiety;

            float tacticsScore = tactics * unit.CurrentAp;
            float ammoAnxietyScore =
                ammoAnxiety * (1f - (float)currentWeapon.CurrentAmmo() / currentWeapon.AmmoCapacity());
			float score = baseScore + tacticsScore + ammoAnxietyScore;

			this.Log($"Reload score: {score} (base: {baseScore}, tactics: {tacticsScore}, ammoAnxiety: {ammoAnxietyScore})");
            results.Add(new AIActionOption(EAIActionType.Reload, score)
            {
                EquipmentAction = EActionType.Reload
            });

            return results;
		}
	}
}

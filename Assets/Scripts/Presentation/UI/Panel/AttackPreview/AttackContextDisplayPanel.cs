using System.Collections.Generic;
using Sirenix.OdinInspector;
using Systems.Damage;
using UnityEngine;

namespace Presentation.UI.Panel.AttackPreview
{
	public class AttackContextDisplayPanel : MonoBehaviour
	{
		[SerializeField, ChildGameObjectsOnly] private List<AttackContextLine> left;
		[SerializeField, ChildGameObjectsOnly] private List<AttackContextLine> right;

		public void Default()
		{
			foreach (var line in left) line.SetDefault();
			foreach (var line in right) line.SetDefault();
		}

		public void Show(DamageExecutingContext context, Systems.Unit.Unit attacker, Dictionary<BodyPartType, DamageExecutingContext> attackContextDict)
		{
			// left
			var hitPrecent = Mathf.Clamp(Mathf.RoundToInt(context.HitRate * 100f), 0, 100);
			left[0].SetPair("命中率", $"{hitPrecent}%");

			var hitInfluences = context.HitRateInfluences;
			for (int i = 1; i < left.Count; i++)
			{
				if (i > hitInfluences.Count)
				{
					left[i].SetDefault();
					continue;
				}
				var influence = hitInfluences[i - 1];
				left[i].SetPair(influence.Item1, influence.Item2);
			}

			// right
            int minDamage = context.FinalDamage[0];
            int maxDamage = context.FinalDamage[0];

            foreach (var theContext in attackContextDict.Values)
            {
                minDamage = Mathf.Min(minDamage, theContext.FinalDamage[0]);
                maxDamage = Mathf.Max(maxDamage, theContext.FinalDamage[0]);
            }
            
			var bulletAmount = context.FinalCalculatedNum;
			right[0].SetPair("伤害", $"({minDamage}~{maxDamage})x{bulletAmount}");

            int maxDefenseDamage = context.FinalDefenseDamage[0];

            foreach (var theContext in attackContextDict.Values)
            {
                maxDefenseDamage = Mathf.Max(maxDefenseDamage, theContext.TotalDefenseDamage);
            }
            
			right[1].SetPair("破甲", $"0~{maxDefenseDamage}");
		}
	}
}

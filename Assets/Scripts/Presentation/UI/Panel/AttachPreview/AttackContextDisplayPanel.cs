using System.Collections.Generic;
using Sirenix.OdinInspector;
using Systems.Damage;
using UnityEngine;

namespace Presentation.UI.Panel.AttachPreview
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

		public void Show(DamageExecutingContext context)
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
			var damage = context.Damage;
			var bulletAmount = context.CalculateNum;
			right[0].SetPair("伤害", $"{damage}x{bulletAmount}");

			var armDamage = context.DefenceDamage;
			right[1].SetPair("破甲", $"{armDamage}");
		}
	}
}

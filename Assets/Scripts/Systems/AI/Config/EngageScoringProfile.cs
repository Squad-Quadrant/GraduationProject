using System;
using UnityEngine;

namespace Systems.AI.Config
{
	[Serializable]
	public class EngageScoringProfile
	{
		[Tooltip("Engage 的基础分。默认 1.0 让其在轴贡献全 0.7 时仍能压过 SearchPlan(0.7)")]
		public float baseScore = 1.0f;

		[Tooltip("自己血量比 → 战斗欲望\n" +
		         "横轴：currentHp / maxHp ∈ [0=死, 1=满血]\n" +
		         "默认线性递增——血越多越想打")]
		public ScoringAxis selfHpAxis = new(curve: AnimationCurve.Linear(0f, 0.2f, 1f, 1f));

		[Tooltip("目标血量比 → 击杀欲望\n" +
		         "横轴：target.currentHp / target.maxHp ∈ [0=死, 1=满血]\n" +
		         "默认线性递减——目标血越少越想打")]
		public ScoringAxis targetHpAxis = new(curve: AnimationCurve.Linear(0f, 1f, 1f, 0.5f));

		[Tooltip("路径距离比 → 攻击意愿\n" +
		         "横轴：pathCost / (moveRange × maxAp) ∈ [0=同位置, 1=满移动力]\n" +
		         "默认线性递减——越远越不想去")]
		public ScoringAxis pathDistanceAxis = new(curve: AnimationCurve.Linear(0f, 1f, 1f, 0.3f));
	}
}

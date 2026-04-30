using System;
using UnityEngine;

namespace Systems.AI.Config
{
	[Serializable]
	public class SearchScoringProfile
	{
		[Tooltip("Search 的基础分。默认 0.7 — 低于 EngagePlan(1.0) 高于 WaitPlan(0.1)")]
		public float baseScore = 0.7f;

		[Tooltip("距离比 → 搜索意愿\n" +
		         "横轴：曼哈顿距离(self, lastKnown) / (moveRange × maxAp) ∈ [0=同位置, 1=一回合刚够]\n" +
		         "默认线性递减——远处情报降优先级")]
		public ScoringAxis pathDistanceAxis = new(curve: AnimationCurve.Linear(0f, 1f, 1f, 0f));

		[Tooltip("情报时效 → 搜索意愿\n" +
		         "横轴：(currentTurn - lastSeenTurn) / 情报失效周期 ∈ [0=刚看到, 1=情报马上时效]\n" +
		         "默认线性递减——越新的情报优先级越高；")]
		public ScoringAxis stalenessAxis = new(curve: AnimationCurve.Linear(0f, 1f, 1f, 0f));
	}
}

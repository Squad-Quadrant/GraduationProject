using System;
using UnityEngine;

namespace Systems.AI.Config
{
	[Serializable]
	public class ReloadScoringProfile
	{
		[Tooltip("Reload 的基础分。默认 0.5")]
		public float baseScore = 0.5f;

		[Tooltip("缺弹度 → 装弹紧迫性\n" +
		         "横轴：1 - currentAmmo / maxAmmo ∈ [0=满弹, 1=空仓]\n" +
		         "默认线性递增——弹越少越想装；\n" +
		         "可设阶梯曲线（比如子弹剩 30% 才急着装）")]
		public ScoringAxis ammoLowAxis = new();
	}
}

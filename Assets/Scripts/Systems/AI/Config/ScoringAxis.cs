using System;
using UnityEngine;

namespace Systems.AI.Config
{
	[Serializable]
	public class ScoringAxis
	{
		[Tooltip("响应曲线：横轴 = 输入值 [0,1]，纵轴 = 输出贡献 [0,1]")]
		public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Tooltip("权重补偿：1 = 该 axis 完全生效；0 = 该 axis 不参与（贡献恒为 1）\n" +
		         "用于让某 axis 影响力减弱")]
		[Range(0f, 1f)]
		public float weight = 1f;

		public ScoringAxis()
		{
		}

		public ScoringAxis(AnimationCurve curve)
		{
			this.curve = curve;
		}

		public float Evaluate(float input)
		{
			float clamped = Mathf.Clamp01(input);
			float curveValue = Mathf.Clamp01(curve.Evaluate(clamped));
			return 1f - (1f - curveValue) * weight;
		}
	}
}

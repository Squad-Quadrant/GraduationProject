using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.AI.Config
{
	[CreateAssetMenu(fileName = "AIBrainConfig", menuName = "Game/AI Brain Config")]
	public class AIBrainConfig : ScriptableObject
	{
		[FoldoutGroup("Movement")]
		[Range(0f, 2f)]
		public float moveBase = 0.3f;

		[FoldoutGroup("Movement"), Tooltip("离敌人距离的权重")]
		[Range(0f, 1f)]
		public float closenessWeight = 0.2f;

		[FoldoutGroup("Movement"), Tooltip("有敌人在攻击范围的权重")]
		[Range(0f, 1f)]
		public float inRangeBonus = 0.2f;

		[FoldoutGroup("Movement")]
		[Range(0f, 0.5f)]
		public float apConservationBonus = 0.1f;


		[FoldoutGroup("Wait")]
		[Range(0f, 0.5f)]
		public float waitScore = 0.1f;
	}
}

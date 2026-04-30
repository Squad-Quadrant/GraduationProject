using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.AI.Config
{
	public enum EIdleBehavior
	{
		Guard = 0,
		Patrol = 1,
	}

	[CreateAssetMenu(fileName = "AIArchetype", menuName = "Game/Unit/AI Archetype")]
	public class AIArchetype : ScriptableObject
	{
		[TitleGroup("Idle 行为")]
		public EIdleBehavior idleBehavior = EIdleBehavior.Guard;

		[TitleGroup("战斗评分 — Engage")]
		[HideLabel]
		public EngageScoringProfile engageProfile = new();

		[TitleGroup("战斗评分 — Reload")]
		[HideLabel]
		public ReloadScoringProfile reloadProfile = new();

		[TitleGroup("战斗评分 — Search")]
		[HideLabel]
		public SearchScoringProfile searchProfile = new();

		[TitleGroup("Wait")]
		[Tooltip("WaitPlan 的固定分。需足够低，让其他 plan 在可行时优先，兜底用")]
		public float waitBaseScore = 0.1f;
	}
}

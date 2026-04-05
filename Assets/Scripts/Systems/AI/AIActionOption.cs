using Data.Runtime;
using UnityEngine;

namespace Systems.AI
{
	public enum EAIActionType
	{
		Wait,
		Move,
		Attack,
        Reload,
        Switch
	}

	public class AIActionOption
	{
		public EAIActionType ActionType { get; }
		public float Score { get; }

		public Vector2Int? MoveTarget { get; set; }
		public string TargetUnitId { get; set; }
		public EActionType EquipmentAction { get; set; }

		public AIActionOption(EAIActionType actionType, float score)
		{
			ActionType = actionType;
			Score = score;
		}

		public override string ToString() =>
			$"[AIOption] {ActionType} score:{Score:F2}" +
			(MoveTarget.HasValue ? $" moveTo:{MoveTarget.Value}" : "") +
			(TargetUnitId != null ? $" target:{TargetUnitId}" : "");
	}
}

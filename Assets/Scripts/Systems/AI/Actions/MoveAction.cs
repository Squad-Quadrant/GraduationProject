using Core.Commands;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Commands;
using UnityEngine;

namespace Systems.AI.Actions
{
	public class MoveAction : IAtomicAction
	{
		public EActionType ActionType => EActionType.Move;

		public Vector2Int Target { get; }

		public MoveAction(Vector2Int target) => Target = target;

		public ICommand CreateCommand(AIContext ctx)
		{
			var pathResult = ctx.ReachableArea.GetPathTo(Target);
			if (!pathResult.Found)
			{
				this.LogWarning($"MoveAction: no path to {Target}");
				return null;
			}

			var unit = ctx.Self;
			int apCost = unit.CalculateMovementApCost(pathResult.TotalCost);
			Debug.Log($"MoveAction: moving {Target} to {apCost}");
			return new MoveUnitCommand(
				unit.id,
				unit.position,
				Target,
				pathResult.Path,
				apCost,
				ctx.UnitService,
				ctx.MapService,
				ctx.EventBus);
		}

		public override string ToString() => $"Move→{Target}";
	}
}

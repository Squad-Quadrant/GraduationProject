using Core.Commands;

namespace Systems.AI.Actions
{
	public class SearchCompletionAction : IAtomicAction
	{
		private readonly string _enemyUnitId;

		public SearchCompletionAction(string enemyUnitId)
		{
			_enemyUnitId = enemyUnitId;
		}

		public ICommand CreateCommand(AIContext ctx)
		{
			var faction = ctx.Self.faction;
			var enemyId = _enemyUnitId;
			var blackboard = ctx.BlackboardService;

			return new LambdaCommand(
				$"SearchCompletion({enemyId})",
				() => blackboard.DismissKnownEnemy(faction, enemyId));
		}

		public override string ToString() => $"SearchComplete→{_enemyUnitId}";
	}
}

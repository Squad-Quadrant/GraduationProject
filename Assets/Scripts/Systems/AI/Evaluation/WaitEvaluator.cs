using System.Collections.Generic;

namespace Systems.AI.Evaluation
{
	public class WaitEvaluator : IActionEvaluator
	{
		private const float WaitScore = 0.1f;

		public List<AIActionOption> Evaluate(AIContext context)
		{
			var brain = context.Brain;

			float waitScore = brain ? brain.waitScore : WaitScore;

			var list = new List<AIActionOption>
			{
				new(EAIActionType.Wait, waitScore)
			};
			return list;
		}
	}
}

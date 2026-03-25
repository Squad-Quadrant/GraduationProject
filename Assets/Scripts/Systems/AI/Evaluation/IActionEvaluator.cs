using System.Collections.Generic;

namespace Systems.AI.Evaluation
{
	public interface IActionEvaluator
	{
		List<AIActionOption> Evaluate(AIContext context);
	}
}

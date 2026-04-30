using System.Collections.Generic;
using Systems.AI.Actions;

namespace Systems.AI.Plans
{
	public interface ITurnPlan
	{
		string Name { get; }

		bool IsViable(AIContext context);

		float Score(AIContext context);

		Queue<IAtomicAction> BuildActionSequence(AIContext context);

		bool ShouldAbort(AIContext context);
	}
}

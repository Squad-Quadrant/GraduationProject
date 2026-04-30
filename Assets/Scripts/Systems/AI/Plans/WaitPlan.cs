using System.Collections.Generic;
using Systems.AI.Actions;

namespace Systems.AI.Plans
{
	public class WaitPlan : ITurnPlan
	{
		public string Name => "Wait";

		public bool IsViable(AIContext context) => true;

		public float Score(AIContext context)
		{
			return context.Archetype ? context.Archetype.waitBaseScore : 0.01f;
		}

		public Queue<IAtomicAction> BuildActionSequence(AIContext context) => new();

		public bool ShouldAbort(AIContext context) => false;
	}
}

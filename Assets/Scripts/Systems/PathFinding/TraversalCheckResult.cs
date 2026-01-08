namespace Systems.PathFinding
{
	public class TraversalCheckResult
	{
		public bool CanPass { get; }

		public bool CanStop { get; }

		public int Cost { get; }

		public TraversalCheckResult(bool canPass, bool canStop, int cost)
		{
			CanPass = canPass;
			CanStop = canStop;
			Cost = cost;
		}

		public static TraversalCheckResult Blocked => new(false, false, int.MaxValue);

		public static TraversalCheckResult Passable(int cost) => new(true, false, cost);

		public static TraversalCheckResult Stoppable(int cost) => new(true, true, cost);

		public override string ToString() =>
			CanPass
				? $"[Traversal] Pass:✓ Stop:{(CanStop ? "✓" : "✗")} Cost:{Cost}"
				: "[Traversal] Blocked";
	}
}

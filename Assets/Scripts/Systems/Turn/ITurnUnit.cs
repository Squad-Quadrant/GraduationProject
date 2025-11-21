namespace Systems.Turn
{
	public interface ITurnUnit
	{
		/// <summary>
		/// Unique identifier for every unit
		/// </summary>
		string Id { get; }

		/// <summary>
		/// determine turn order (dynamic)
		/// </summary>
		int Speed { get; }

		/// <summary>
		/// Indicates if the unit can act this turn
		/// </summary>
		bool CanAct { get; }

		/// <summary>
		/// determine turn order when speed is equal
		/// </summary>
		int ActionPriority { get; set; }
	}
}

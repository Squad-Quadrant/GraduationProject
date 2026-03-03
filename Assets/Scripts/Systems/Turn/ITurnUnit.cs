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
		bool CanAct { get; } // 这个属性如果为false，单位会被跳过但不会被移出队列（比如眩晕状态），或许也可以在状态机里面判断，这样可以加上演出

		/// <summary>
		/// determine turn order (before speed)
		/// </summary>
		int ActionPriority { get; set; }
	}
}

namespace Data.Runtime.Events
{
	/// <summary>
	/// Types of actions a unit can perform during their turn.
	/// </summary>
	public enum EActionType
	{
		None = 0,
		EndTurn,
		Move,
		Wait,
		Interact,
		UseItem,
		Attack,
		Defend,
		// ...
	}
}

namespace Data.Runtime
{
	/// <summary>
	/// Types of actions a unit can perform during their turn.
	/// </summary>
	public enum EActionType
	{
		None = 0,
		Move,
		Wait,
		Interact,
		MainWeapon,
        SecondaryWeapon,
        TacticalItem0,
        TacticalItem1,
        TacticalItem2,
		Defend,
        //...
		Count,
	}
}

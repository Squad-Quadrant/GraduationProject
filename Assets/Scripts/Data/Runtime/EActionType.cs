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
        Attack,
        UseTacticalItem,
        UseSkill,
		Defend,
        Reload,
        SwitchWeapon,
        Back,
        AI,
        //...
		Count,
	}
}

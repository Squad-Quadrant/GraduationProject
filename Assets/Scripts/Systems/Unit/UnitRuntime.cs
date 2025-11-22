using System;
using System.Collections.Generic;
using Systems.Equipment;

namespace Systems.Unit
{
	/// <summary>
	/// Runtime values that can frequently change during gameplay
	/// </summary>
	[Serializable]
	public class UnitRuntime : IEquipable
	{
		public int currentHp;

		public bool StillAlive => currentHp > 0;

		public bool isStunned;
		
		#region IEquipable

		// todo:武器道具初始化
		public WeaponInfo MainWeapon { get; set; }
		public WeaponInfo SecondaryWeapon { get; set; }
		public TacticalItemInfo TacticalItemInfo0 { get; set; }
		public TacticalItemInfo TacticalItemInfo1 { get; set; }
		public TacticalItemInfo TacticalItemInfo2 { get; set; }
		public List<TacticalItemInfo> TacticalItemInfos => new()
		{
			TacticalItemInfo0,
			TacticalItemInfo1,
			TacticalItemInfo2
		};

		#endregion

		public void Initialize(int maxHp)
		{
			currentHp = maxHp;
			isStunned = false;
		}
	}
}

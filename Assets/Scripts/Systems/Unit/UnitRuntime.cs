using System;

namespace Systems.Unit
{
	/// <summary>
	/// Runtime values that can frequently change during gameplay
	/// </summary>
	[Serializable]
	public class UnitRuntime
	{
		public int currentHp;

		public bool StillAlive => currentHp > 0;

		public bool isStunned;

		public void Initialize(int maxHp)
		{
			currentHp = maxHp;
			isStunned = false;
		}
	}
}

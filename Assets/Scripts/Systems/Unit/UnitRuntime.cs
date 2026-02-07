using System;
using UnityEngine;

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

        public Vector2Int position;
        
        public Unit Owner { get; set; }

		public void Initialize(int maxHp, Vector2Int position, Unit owner)
		{
			currentHp = maxHp;
			isStunned = false;
            this.position = position;
            Owner = owner;
		}
	}
}

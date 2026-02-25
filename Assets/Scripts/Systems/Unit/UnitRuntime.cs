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
        
        [field: NonSerialized] public Unit Owner { get; set; }

		public void Initialize(int maxHp, Vector2Int pos, Unit owner)
		{
			currentHp = maxHp;
			isStunned = false;
            position = pos;
            Owner = owner;
		}
	}
}

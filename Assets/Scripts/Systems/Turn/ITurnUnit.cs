using Systems.Unit;
using UnityEngine;

namespace Systems.Turn
{
	public interface ITurnUnit
	{
		string Id { get; }

		int Speed { get; }

		bool CanAct { get; } // 这个属性如果为false，单位会被跳过但不会被移出队列（比如眩晕状态），或许也可以在状态机里面判断，这样可以加上演出

		int ActionPriority { get; set; } // 优先级高于速度，数值越大优先级越高，默认0

		EUnitFaction Faction { get; }

		string DisplayName { get; }

		Sprite DisplayIcon { get; }

		Vector2Int CellPosition { get; }

		void OnTurnStart();
	}
}

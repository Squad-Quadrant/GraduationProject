using System.Collections.Generic;

namespace Systems.Turn
{
	public interface ITurnService
	{
		// Turn Lifecycle（一个Turn指所有单位全部行动一次的一个大回合）
		int TurnNumber { get; }
		bool IsTurnActive { get; }
		void StartTurn();
		void EndTurn();

		// UnitTurn Lifecycle
		ITurnUnit ActiveUnit { get; }
		bool IsUnitActing { get; }
		bool IsTurnComplete { get; } // 当目前没有可行动的单位时
		ITurnUnit NextUnit();
		void EndUnitTurn();

		// Unit Order Management
		void AddUnit(ITurnUnit unit);
		void RemoveUnit(string unitId);
		void SetUnitPriority(string unitId, int priority);
		void MoveUnitToNext(string unitId);
		void ResortQueue();
		void Clear();

		// Query
		IReadOnlyList<ITurnUnit> GetFullOrder();
		IReadOnlyList<ITurnUnit> GetUpcoming();
		int IndexOf(string unitId);
	}
}

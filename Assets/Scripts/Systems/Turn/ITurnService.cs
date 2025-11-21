using System.Collections.Generic;

namespace Systems.Turn
{
	public interface ITurnService
	{
		void StartTurn();

		void EndTurn();

		int GetCurrentTurnNumber();

		bool IsTurnActive();

		/// <summary>
		/// Switches to the next unit in the turn order and returns it.
		/// </summary>
		/// <returns></returns>
		ITurnUnit NextUnit();

		void EndUnitTurn();

		ITurnUnit GetCurrentUnit();

		bool HasCurrentUnit();

		void RegisterUnit(ITurnUnit unit);

		void UnregisterUnit(string unitId);

		bool IsUnitRegistered(string unitId);

		void UpdateUnitSpeed(string unitId);

		void ForceInsertUnit(string unitId, int priority = 999);

		IReadOnlyList<ITurnUnit> GetTurnOrder();

		int GetUnitPositionInQueue(string unitId);

		int GetRemainingUnitsCount();

		bool IsCurrentTurnComplete();

		void Clear();
	}
}

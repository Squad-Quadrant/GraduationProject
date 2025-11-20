using System;
using System.Collections.Generic;
using Data.Config;
using UnityEngine;

namespace Systems.Unit
{
	public interface IUnitService
	{
		int Count { get; }

		Unit CreateUnit(string unitId, UnitConfig config, Vector2Int position);

		void DestroyUnit(string unitId, string killerUnitId = null);

		/// <summary>
		/// Just clear all units' data without firing events.
		/// </summary>
		void Clear();

		/// <summary>
		/// Get unit by ID. Throws exception if not found.
		/// </summary>
		Unit GetUnit(string unitId);

		/// <summary>
		/// Try to get unit by ID. Returns false if not found.
		/// </summary>
		bool TryGetUnit(string unitId, out Unit unit);

		bool HasUnit(string unitId);

		IReadOnlyList<Unit> GetAllUnits();

		IReadOnlyList<Unit> GetAllAliveUnits();

		/// <summary>
		/// Get all units within a certain range based on Manhattan distance.
		/// </summary>
		IReadOnlyList<Unit> GetUnitsInRange(Vector2Int center, int range, bool includeCenter = true);

		IReadOnlyList<Unit> GetUnitsWhere(Func<Unit, bool> predicate);
	}
}

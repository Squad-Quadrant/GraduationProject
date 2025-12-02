using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Data.Config;
using Data.Runtime.Events;
using Data.Runtime.Events.Unit;
using UnityEngine;

namespace Systems.Unit
{
	public class UnitService : IUnitService
	{
		private readonly IEventBus _eventBus;

		public UnitService(IEventBus eventBus)
		{
			_eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));
			Debug.Log("[UnitService] Initialized");
		}

		private readonly Dictionary<string, Unit> _units = new();

		public int Count => _units.Count;

		public Unit CreateUnit(string unitId, UnitConfig config, Vector2Int position)
		{
			if (string.IsNullOrEmpty(unitId))
				throw new ArgumentException("[UnitService] Unit ID cannot be null or empty.", nameof(unitId));

			if (!config)
				throw new ArgumentNullException(nameof(config), "[UnitService] UnitConfig cannot be null.");

			if (_units.ContainsKey(unitId))
				throw new InvalidOperationException($"[UnitService] A unit with ID '{unitId}' already exists.");

			var unit = Unit.LoadFromConfig(unitId, config, position);
			_units[unitId] = unit;
			Debug.Log($"[UnitService] Created unit: {unit}");
			_eventBus.Publish(new UnitCreatedEvent(unit));
			return unit;
		}

		public void DestroyUnit(string unitId, string killerUnitId = null)
		{
			if (!_units.Remove(unitId, out var unit))
			{
				Debug.LogWarning($"[UnitService] Attempted to destroy non-existent unit: {unitId}");
				return;
			}

			Debug.Log($"[UnitService] Unit destroyed: {unit.name}({unitId})" +
			          (killerUnitId != null ? $" by {killerUnitId}" : ""));

			_eventBus.Publish(new UnitDestroyedEvent(unit, killerUnitId));
		}

		public void Clear()
		{
			int count = _units.Count;
			_units.Clear();
			Debug.Log($"[UnitService] Cleared {count} units");
		}

		public Unit GetUnit(string unitId)
		{
			if (_units.TryGetValue(unitId, out var unit))
				return unit;
			throw new KeyNotFoundException($"[UnitService] No unit found with ID '{unitId}'.");
		}

		public bool TryGetUnit(string unitId, out Unit unit) => _units.TryGetValue(unitId, out unit);

		public bool HasUnit(string unitId) => _units.ContainsKey(unitId);

		public IReadOnlyList<Unit> GetAllUnits() =>
			_units.Values.ToList();

		public IReadOnlyList<Unit> GetAllAliveUnits() =>
			_units.Values.Where(u => u.runtime.StillAlive).ToList();

		public IReadOnlyList<Unit> GetUnitsInRange(Vector2Int center, int range, bool includeCenter = true)
		{
			if (range < 0)
				throw new ArgumentOutOfRangeException(nameof(range));

			return _units.Values
				.Where(u =>
				{
					int distance = Math.Abs(u.position.x - center.x) + Math.Abs(u.position.y - center.y);
					if (!includeCenter && distance == 0)
						return false;
					return distance <= range;
				})
				.ToList();
		}

		public IReadOnlyList<Unit> GetUnitsWhere(Func<Unit, bool> predicate) =>
			predicate == null ?
				throw new ArgumentNullException(nameof(predicate)) :
				_units.Values.Where(predicate).ToList();
	}
}

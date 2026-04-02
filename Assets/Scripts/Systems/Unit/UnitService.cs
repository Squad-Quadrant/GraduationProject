using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data;
using Data.Config;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Unit;
using UnityEngine;

namespace Systems.Unit
{
	public class UnitService : IUnitService, IDisposable
	{
		private readonly IEventBus _eventBus;
        private readonly DataManager _dataManager;

		public UnitService(IEventBus eventBus, DataManager dataManager)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
			this.Log("Initialized");
		}

        public void Dispose()
        {
            this.Log("Disposed");
        }

		private readonly Dictionary<string, Unit> _units = new();

		public int Count => _units.Count;

		public Unit CreateUnit(string unitId, UnitConfig config, Vector2Int position)
		{
			if (string.IsNullOrEmpty(unitId))
				throw new ArgumentException("Unit ID cannot be null or empty.", nameof(unitId));

			if (!config)
				throw new ArgumentNullException(nameof(config), "UnitConfig cannot be null.");

			if (_units.ContainsKey(unitId))
				throw new InvalidOperationException($"A unit with ID '{unitId}' already exists.");

			var unit = Unit.LoadFromConfig(unitId, config, position, _eventBus);
            var equipmentConig = _dataManager.GetEquipmentConfigList(unitId);
            unit.InitEquipment(equipmentConig);
			_units[unitId] = unit;
			this.Log($"Created unit: {unit}");
			_eventBus.Publish(new UnitCreatedEvent(unit));
			return unit;
		}

		public void DestroyUnit(string unitId, string killerUnitId = null)
		{
			if (!_units.Remove(unitId, out var unit))
			{
				this.LogWarning($"Attempted to destroy non-existent unit: {unitId}");
				return;
			}

			this.Log($"Unit destroyed: {unit.name}({unitId})" +
			          (killerUnitId != null ? $" by {killerUnitId}" : ""));
            
            this.Log($"{unit.name}死亡", true);

			_eventBus.Publish(new UnitDestroyedEvent(unit, killerUnitId));
		}

		public void Clear()
		{
			int count = _units.Count;
			_units.Clear();
			this.Log($"Cleared {count} units");
		}

		public Unit GetUnit(string unitId)
		{
			if (_units.TryGetValue(unitId, out var unit))
				return unit;
			throw new KeyNotFoundException($"No unit found with ID: {unitId}");
		}

		public bool TryGetUnit(string unitId, out Unit unit) => _units.TryGetValue(unitId, out unit);

		public bool HasUnit(string unitId) => _units.ContainsKey(unitId);

		public IReadOnlyList<Unit> GetAllUnits() =>
			_units.Values.ToList();

		public IReadOnlyList<Unit> GetAllAliveUnits() =>
			_units.Values.Where(u => u.IsAlive).ToList();

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
        
        public IReadOnlyList<Unit> GetUnitsInDistance(Vector2Int center, int range, bool includeCenter = false)
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

        public Unit GetUnitAtPosition(Vector2Int position)
        {
            return _units.Values.FirstOrDefault(u => u.position == position);
        }
        
        public void CheckUnitDeath()
        {
            var deadUnits = _units.Values.Where(u => u.IsAlive == false).ToList();
            foreach (var unit in deadUnits)
            {
                DestroyUnit(unit.id);
                this.Log($"Unit '{unit.name}'({unit.id}) has died.");
            }
        }
    }
}

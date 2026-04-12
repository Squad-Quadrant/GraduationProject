using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Vision;
using Systems.Unit;
using UnityEngine;

namespace Systems.Vision
{
	public class VisionService : IVisionService
	{
		private readonly IEventBus _eventBus;
		private readonly IVisionCalculator _calculator;
		private readonly IUnitService _unitService;

		private HashSet<Vector2Int> _baseVisibleCells = new();
		private HashSet<Vector2Int> _mergedVisibleCells = new(); // base vision + temporary reveals
		private readonly Dictionary<int, HashSet<Vector2Int>> _temporaryReveals = new(); // tokenId → revealed cells
		private readonly Dictionary<string, Vector2Int> _spottedEnemies = new(); // unitId → last known position

		public IReadOnlyCollection<Vector2Int> CurrentVisibleCells => _mergedVisibleCells;
		public IReadOnlyDictionary<string, Vector2Int> SpottedEnemies => _spottedEnemies;

		public VisionService(IEventBus eventBus, IVisionCalculator calculator, IUnitService unitService)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));

			this.Log("Initialized");
		}

		public bool IsCellVisible(Vector2Int cell) => _mergedVisibleCells.Contains(cell);

		public bool IsEnemySpotted(string unitId) => _spottedEnemies.ContainsKey(unitId);
		public Vector2Int? GetSpottedPosition(string unitId) => _spottedEnemies.TryGetValue(unitId, out var pos) ? pos : null;

		public void UpdateVisionForUnit(Unit.Unit unit)
		{
			var cell = _calculator.CalculateVisibleCells(unit.position, unit.visionRange);
			_baseVisibleCells = cell;
			RecalculateMergedAndPublish(unit.id);
		}

		public void UpdateVisionAtPosition(Vector2Int position, int visionRange, string unitId)
		{
			var cell = _calculator.CalculateVisibleCells(position, visionRange);
			_baseVisibleCells = cell;
			RecalculateMergedAndPublish(unitId);
		}

		public void UpdateVisionByPrecomputed(HashSet<Vector2Int> cells, string unitId)
		{
			_baseVisibleCells = cells;
			RecalculateMergedAndPublish(unitId);
		}

		public int AddTemporaryReveal(IReadOnlyList<Vector2Int> cells)
		{
			var token = Guid.NewGuid().GetHashCode();
			_temporaryReveals[token] = new HashSet<Vector2Int>(cells);

			this.Log($"Added temporary reveal {token} ({cells.Count} cells)");

			RecalculateMergedAndPublish(null);
			return token;
		}

		public void RemoveTemporaryReveal(int token)
		{
			if (!_temporaryReveals.Remove(token)) return;

			this.Log($"Removed temporary reveal {token}");

			RecalculateMergedAndPublish(null);
		}

		public void MarkEnemySpotted(string unitId, Vector2Int position)
		{
			bool isNew = !_spottedEnemies.ContainsKey(unitId);
			_spottedEnemies[unitId] = position;

			if (!isNew) return;

			this.Log($"Enemy '{unitId}' spotted at {position}");
			_eventBus.Publish(new EnemySpottedEvent(unitId, position));
		}

		public void ClearSpottedMark(string unitId)
		{
			if (!_spottedEnemies.Remove(unitId)) return;

			this.Log($"Cleared spotted mark for '{unitId}'");
			_eventBus.Publish(new EnemySpotClearedEvent(unitId));
		}

		private void RecalculateMergedAndPublish(string unitId)
		{
			// var previousMerged = _mergedVisibleCells;

			if (_temporaryReveals.Count == 0)
				_mergedVisibleCells = _baseVisibleCells;
			else
			{
				var merged = new HashSet<Vector2Int>(_baseVisibleCells);
				foreach (var reveal in _temporaryReveals.Values)
					merged.UnionWith(reveal);
				_mergedVisibleCells = merged;
			}

			// detect newly visible enemies and mark them as spotted
			// foreach (var cell in _mergedVisibleCells)
			// {
			// 	if (previousMerged.Contains(cell)) continue;
			//
			// 	var occupant = _unitService.GetUnitAtPosition(cell);
			// 	if (occupant is not { IsAlive: true }) continue;
			// 	if (occupant.faction != EUnitFaction.Enemy) continue;
			//
			// 	MarkEnemySpotted(occupant.id, cell);
			// }

			_eventBus.Publish(new VisionChangedEvent(_mergedVisibleCells, unitId));
		}
	}
}

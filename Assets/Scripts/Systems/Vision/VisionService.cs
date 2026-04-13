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

		private readonly Dictionary<string, HashSet<Vector2Int>> _perUnitVision = new();
		private HashSet<Vector2Int> _mergedVisibleCells = new(); // per unit vision + temporary reveals
		private readonly Dictionary<int, HashSet<Vector2Int>> _temporaryReveals = new(); // tokenId → revealed cells
		private readonly Dictionary<string, Vector2Int> _spottedEnemies = new(); // unitId → last known position

		private int _nextTokenId = 1;

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

		public void RecalculateSharedVision()
		{
			_perUnitVision.Clear();

			var allUnits = _unitService.GetAllAliveUnits();
			foreach (var unit in allUnits)
			{
				if (unit.faction != EUnitFaction.Player) continue;

				var cells = _calculator.CalculateVisibleCells(unit.position, unit.visionRange);
				_perUnitVision[unit.id] = cells;
			}

			this.Log($"Full recalc: {_perUnitVision.Count} friendly units");
			RebuildMergedAndPublish();
		}

		public void UpdateUnitVision(string unitId, Vector2Int position, int visionRange)
		{
			var cells = _calculator.CalculateVisibleCells(position, visionRange);
			_perUnitVision[unitId] = cells;

			RebuildMergedAndPublish();
		}

		public void RemoveUnitVision(string unitId)
		{
			if (!_perUnitVision.Remove(unitId)) return;

			this.Log($"Removed vision for unit '{unitId}'");
			RebuildMergedAndPublish();
		}

		public RevealToken AddTemporaryReveal(IReadOnlyList<Vector2Int> cells)
		{
			var token = new RevealToken(_nextTokenId++);
			_temporaryReveals[token.Id] = new HashSet<Vector2Int>(cells);
			this.Log($"Added temporary reveal {token} ({cells.Count} cells)");
			RebuildMergedAndPublish();
			return token;
		}

		public void RemoveTemporaryReveal(RevealToken token)
		{
			if (!token.IsValid) return;
			if (!_temporaryReveals.Remove(token.Id)) return;
			this.Log($"Removed temporary reveal {token}");
			RebuildMergedAndPublish();
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

		private void RebuildMergedAndPublish()
		{
			var previous = _mergedVisibleCells;

			var merged = new HashSet<Vector2Int>();
			foreach (var unitCells in _perUnitVision.Values)
				merged.UnionWith(unitCells);

			foreach (var reveal in _temporaryReveals.Values)
				merged.UnionWith(reveal);

			_mergedVisibleCells = merged;

			DetectNewlyVisibleEnemies(previous, _mergedVisibleCells);

			_eventBus.Publish(new VisionChangedEvent(_mergedVisibleCells));
		}

		private void DetectNewlyVisibleEnemies(HashSet<Vector2Int> previousVisible, HashSet<Vector2Int> currentVisible)
		{
			foreach (var cell in currentVisible)
			{
				if (previousVisible.Contains(cell)) continue;

				var occupant = _unitService.GetUnitAtPosition(cell);
				if (occupant is not { IsAlive: true }) continue;
				if (occupant.faction != EUnitFaction.Enemy) continue;

				MarkEnemySpotted(occupant.id, cell);
			}
		}
	}
}

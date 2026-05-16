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
		private readonly Dictionary<int, HashSet<Vector2Int>> _visionBlockers = new(); // tokenId → vision blocker cells
		private HashSet<Vector2Int> _allBlockerCells = new(); // merged from visionBlockers
		private readonly Dictionary<string, Vector2Int> _spottedEnemies = new(); // unitId → last known position

		private int _nextTokenId = 1;
		private int _nextBlockerTokenId = 1;

		public IReadOnlyCollection<Vector2Int> CurrentVisibleCells => _mergedVisibleCells;

		public VisionService(IEventBus eventBus, IVisionCalculator calculator, IUnitService unitService)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));

			this.Log("Initialized");
		}

		public bool IsCellVisible(Vector2Int cell) => _mergedVisibleCells.Contains(cell);

		public void RecalculateSharedVision()
		{
			_perUnitVision.Clear();

			var allUnits = _unitService.GetAllAliveUnits();
			foreach (var unit in allUnits)
			{
				if (unit.faction != EUnitFaction.Player) continue;

				var cells = _calculator.CalculateVisibleCells(
					unit.position,
					unit.visionRange,
					visionBlockers: _allBlockerCells);
				_perUnitVision[unit.id] = cells;
			}

			this.Log($"Full recalc: {_perUnitVision.Count} friendly units");
			RebuildMergedAndPublish();
		}

		public void UpdateUnitVision(string unitId, Vector2Int position, int visionRange)
		{
			var cells = _calculator.CalculateVisibleCells(
				position,
				visionRange,
				visionBlockers: _allBlockerCells);
			_perUnitVision[unitId] = cells;

			RebuildMergedAndPublish();
		}

		public void RemoveUnitVision(string unitId)
		{
			if (!_perUnitVision.Remove(unitId)) return;

			this.Log($"Removed vision for unit '{unitId}'");
			RebuildMergedAndPublish();
		}

		#region Temporary Reveals

		public IReadOnlyDictionary<string, Vector2Int> SpottedEnemies => _spottedEnemies;

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

		#endregion

		#region Vision Blockers

		public IReadOnlyCollection<Vector2Int> VisionBlockingCells => _allBlockerCells;

		public VisionBlockerToken AddVisionBlocker(IReadOnlyList<Vector2Int> cells)
		{
			var token = new VisionBlockerToken(_nextBlockerTokenId++);
			_visionBlockers[token.Id] = new HashSet<Vector2Int>(cells);
			RebuildBlockerCache();

			this.Log($"Added vision blocker {token} ({cells.Count} cells); total blocker cells = {_allBlockerCells.Count}");
			RecalculateSharedVision();
			return token;
		}

		public void RemoveVisionBlocker(VisionBlockerToken token)
		{
			if (!token.IsValid) return;
			if (!_visionBlockers.Remove(token.Id)) return;
			RebuildBlockerCache();
			this.Log($"Removed vision blocker {token}; total blocker cells = {_allBlockerCells.Count}");
			RecalculateSharedVision();
		}

		private void RebuildBlockerCache()
		{
			_allBlockerCells = new HashSet<Vector2Int>();
			foreach (var set in _visionBlockers.Values)
				_allBlockerCells.UnionWith(set);
		}

		#endregion

		#region Enemy Spotting

		public bool IsEnemySpotted(string unitId) => _spottedEnemies.ContainsKey(unitId);

		public Vector2Int? GetSpottedPosition(string unitId) => _spottedEnemies.TryGetValue(unitId, out var pos) ? pos : null;

		public void MarkEnemySpotted(string unitId, Vector2Int position)
		{
			bool isNew = !_spottedEnemies.ContainsKey(unitId);
			_spottedEnemies[unitId] = position;

			if (!isNew) return;

			this.Log($"Enemy '{unitId}' spotted at {position}");
			_eventBus.Publish(new EnemySpottedEvent(unitId, position));

			RebuildMergedAndPublish();
		}

		public void ClearSpottedMark(string unitId)
		{
			if (!_spottedEnemies.Remove(unitId)) return;

			this.Log($"Cleared spotted mark for '{unitId}'");
			_eventBus.Publish(new EnemySpotClearedEvent(unitId));

			RebuildMergedAndPublish();
		}

		#endregion

		private void RebuildMergedAndPublish()
		{
			var previous = _mergedVisibleCells;

			var merged = new HashSet<Vector2Int>();
            
            // 单位视野
            foreach (var unitCells in _perUnitVision.Values)
                merged.UnionWith(unitCells);
            
            // 临时揭示
			foreach (var reveal in _temporaryReveals.Values)
				merged.UnionWith(reveal);

            // 单位位置
			var allUnits = _unitService.GetAllAliveUnits();
			foreach (var unit in allUnits)
			{
				if (unit.faction == EUnitFaction.Player)
					merged.Add(unit.position);
			}

			foreach (var pos in _spottedEnemies.Values)
				merged.Add(pos);

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

using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.AI;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Map;
using Systems.AI.Blackboard;
using Systems.Map.Region;
using Systems.Unit;
using Systems.Vision;

namespace Systems.AI.Alert
{
	public class AlertService : IAlertService, IDisposable
	{
		private readonly IEventBus _eventBus;
		private readonly IUnitService _unitService;
		private readonly IVisionCalculator _visionCalculator;
		private readonly IRegionService _regionService;
		private readonly IAIBlackboardService _blackboardService;

		private readonly Dictionary<string, EAlertLevel> _currentLevels = new(); // unitId -> alterLevel

		public AlertService(
			IEventBus eventBus,
			IUnitService unitService,
			IVisionCalculator visionCalculator,
			IRegionService regionService,
			IAIBlackboardService blackboardService)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
			_visionCalculator = visionCalculator ?? throw new ArgumentNullException(nameof(visionCalculator));
			_regionService = regionService ?? throw new ArgumentNullException(nameof(regionService));
			_blackboardService = blackboardService ?? throw new ArgumentNullException(nameof(blackboardService));

			_eventBus.Subscribe<BlackboardUpdatedEvent>(OnBlackboardUpdated);
			_eventBus.Subscribe<UnitMovedEvent>(OnUnitMoved);
			_eventBus.Subscribe<RegionUnlockedEvent>(OnRegionUnlocked);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<BlackboardUpdatedEvent>(OnBlackboardUpdated);
			_eventBus.Unsubscribe<UnitMovedEvent>(OnUnitMoved);
			_eventBus.Unsubscribe<RegionUnlockedEvent>(OnRegionUnlocked);
		}

		public EAlertLevel GetAlertLevel(string unitId) => _currentLevels.GetValueOrDefault(unitId, EAlertLevel.Calm);

		private void OnBlackboardUpdated(BlackboardUpdatedEvent e) => RecomputeForFaction(e.Faction);

		private void OnUnitMoved(UnitMovedEvent e)
		{
			if (e.Unit == null) return;
			if (e.Unit.faction == EUnitFaction.Player) return;
			RecomputeForUnit(e.Unit);
		}

		private void OnRegionUnlocked(RegionUnlockedEvent e) => RecomputeAll();

		private void RecomputeAll()
		{
			foreach (var unit in _unitService.GetAllAliveUnits())
			{
				if (unit.faction is EUnitFaction.Player or EUnitFaction.None) continue;
				RecomputeForUnit(unit);
			}
		}

		private void RecomputeForFaction(EUnitFaction faction)
		{
			foreach (var unit in _unitService.GetAllAliveUnits())
			{
				if (unit.faction is EUnitFaction.Player or EUnitFaction.None) continue;
				if (unit.faction != faction) continue;
				RecomputeForUnit(unit);
			}
		}

		private void RecomputeForUnit(Unit.Unit unit)
		{
			var newLevel = ComputeAlertLevel(unit);

			// 字典里没有 = 第一次见到这个 unit，视为 Calm
			var oldLevel = _currentLevels.TryGetValue(unit.id, out var v) ? v : EAlertLevel.Calm;

			if (oldLevel == newLevel) return;

			_currentLevels[unit.id] = newLevel;

			this.Log($"'{unit.id}' alert: {oldLevel} → {newLevel}");

			_eventBus.Publish(new UnitAlertStateChangedEvent(
				unitId: unit.id,
				faction: unit.faction,
				from: oldLevel,
				to: newLevel,
				position: unit.position));
		}

		private EAlertLevel ComputeAlertLevel(Unit.Unit unit)
		{
			// 如果区域未解锁，不做任何反应
			if (!_regionService.IsCellUnlocked(unit.position)) return EAlertLevel.Calm;

			var board = _blackboardService.GetBlackboard(unit.faction);
			if (board == null || board.KnownEnemies.Count == 0) return EAlertLevel.Calm;

			var visible = _visionCalculator.CalculateVisibleCells(unit.position, unit.visionRange);

			foreach (var known in board.KnownEnemies.Values)
			{
				if (!_unitService.TryGetUnit(known.EnemyUnitId, out var enemyUnit) || !enemyUnit.IsAlive) continue;
				if (visible.Contains(enemyUnit.position)) return EAlertLevel.Combat;
			}
			return EAlertLevel.Alert;
		}
	}
}

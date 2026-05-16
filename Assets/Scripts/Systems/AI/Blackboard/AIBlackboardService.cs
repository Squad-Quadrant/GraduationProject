using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.Unit;
using Systems.Turn;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;

namespace Systems.AI.Blackboard
{
	public class AIBlackboardService : IAIBlackboardService, IDisposable
	{
		private const int KnownEnemyExpirationTurns = 5;
		private const int ThreatExpirationTurns = 2; // 威胁记录的过期窗口（回合数）

		private readonly IEventBus _eventBus;
		private readonly IUnitService _unitService;
		private readonly ITurnService _turnService;
		private readonly IVisionCalculator _visionCalculator;
		private readonly IVisionService _visionService;

		private readonly Dictionary<EUnitFaction, AIBlackboard> _blackboards = new();

		public AIBlackboardService(
			IEventBus eventBus,
			IUnitService unitService,
			ITurnService turnService,
			IVisionCalculator visionCalculator,
			IVisionService visionService)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
			_turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
			_visionCalculator = visionCalculator ?? throw new ArgumentNullException(nameof(visionCalculator));
			_visionService = visionService ?? throw new ArgumentNullException(nameof(visionService));

			_eventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
			_eventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
			_eventBus.Subscribe<UnitMovedEvent>(OnUnitMoved);
			_eventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
			_eventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
			_eventBus.Unsubscribe<UnitMovedEvent>(OnUnitMoved);
			_eventBus.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);
		}

		public AIBlackboard GetBlackboard(EUnitFaction faction)
		{
			if (faction is EUnitFaction.Player or EUnitFaction.None)
			{
				this.LogWarning($"GetBlackboard called with non-AI faction '{faction}'");
				return null;
			}

			if (_blackboards.TryGetValue(faction, out var board)) return board;
			board = new AIBlackboard(faction, _eventBus);
			_blackboards[faction] = board;
			this.Log($"Lazily created blackboard for faction '{faction}'");
			return board;
		}

		public void ReportVisibleEnemies(
			EUnitFaction faction,
			int currentTurn,
			string reporterUnitId,
			IEnumerable<Unit.Unit> visibleEnemies)
		{
			if (visibleEnemies == null) return;

			var board = GetBlackboard(faction);
			if (board == null) return;

			int count = 0;
			foreach (var enemy in visibleEnemies)
			{
				if (enemy == null) continue;
				board.UpdateKnownEnemy(enemy.id, enemy.position, currentTurn, reporterUnitId);
				count++;
			}

			if (count > 0)
				this.Log($"'{reporterUnitId}' reported {count} visible enemies to '{faction}' blackboard");
		}

		public void DismissKnownEnemy(EUnitFaction faction, string enemyUnitId)
		{
			var board = GetBlackboard(faction);
			if (board == null) return;
			if (board.RemoveKnownEnemy(enemyUnitId))
				this.Log($"Dismissed KnownEnemy '{enemyUnitId}' from '{faction}' blackboard");
		}

		private void OnDamageApplied(DamageAppliedEvent e)
		{
			var ctx = e.Context;
			if (ctx?.Defender == null) return;

			var defender = ctx.Defender;
			var board = GetBlackboard(defender.faction);
			if (board == null) return;

			string attackerId = null;
			Vector2Int? attackerPos = null;
			if (ctx.Attacker is Unit.Unit attackerUnit)
			{
				attackerId = attackerUnit.id;
				attackerPos = attackerUnit.position;
			}

			int turn = _turnService.TurnNumber;

			board.RecordThreat(new ThreatRecord(
				defender.id,
				defender.position,
				turn: turn,
				attackerId,
				attackerPos
			));

			if (attackerId != null)
				board.UpdateKnownEnemy(attackerId, attackerPos.Value, turn, defender.id);
		}

		private void OnTurnStarted(TurnStartedEvent e)
		{
			int totalThreatPurged = 0;
			int totalEnemyPurged = 0;
			foreach (var board in _blackboards.Values)
			{
				totalThreatPurged += board.PurgeExpiredThreats(e.TurnNumber, ThreatExpirationTurns);
				totalEnemyPurged += board.PurgeExpiredKnownEnemies(e.TurnNumber, KnownEnemyExpirationTurns);
			}

			if (totalThreatPurged > 0)
				this.Log($"Purged {totalThreatPurged} expired threats at turn {e.TurnNumber}");
			if (totalEnemyPurged > 0)
				this.Log($"Purged {totalEnemyPurged} expired KnownEnemies at turn {e.TurnNumber}");
		}

		private void OnUnitMoved(UnitMovedEvent e)
		{
			if (e.Unit is not { faction: EUnitFaction.Player }) return;
			if (e.Path == null || e.Path.Count == 0) return;

			ScanPathAgainstAllAIVision(e.Unit, e.Path);
		}

		private void OnLevelLoaded(LevelLoadedEvent e)
		{
			int currentTurn = _turnService.TurnNumber;
			int totalReports = 0;

			foreach (var aiUnit in _unitService.GetAllAliveUnits())
			{
				if (aiUnit.faction is EUnitFaction.Player or EUnitFaction.None) continue;

				var board = GetBlackboard(aiUnit.faction);
				if (board == null) continue;

				var aiVisible = AIVisionHelper.CalculateVisibleCells(aiUnit, _visionCalculator, _visionService);

				foreach (var other in _unitService.GetAllAliveUnits())
				{
					if (!aiUnit.IsHostile(other)) continue;
					if (!aiVisible.Contains(other.position)) continue;
					board.UpdateKnownEnemy(other.id, other.position, currentTurn, aiUnit.id);
					totalReports++;
				}
			}

			this.Log($"Initial scan completed: {totalReports} known-enemy entries written");
		}

		private void ScanPathAgainstAllAIVision(Unit.Unit movingPlayer, IReadOnlyList<Vector2Int> path)
		{
			int currentTurn = _turnService.TurnNumber;

			foreach (var aiUnit in _unitService.GetAllAliveUnits())
			{
				if (aiUnit.faction is EUnitFaction.Player or EUnitFaction.None) continue;
				if (aiUnit.id == movingPlayer.id) continue;
				if (!aiUnit.IsHostile(movingPlayer)) continue;

				var board = GetBlackboard(aiUnit.faction);
				if (board == null) return;

				var aiVisible = AIVisionHelper.CalculateVisibleCells(aiUnit, _visionCalculator, _visionService);

				Vector2Int? lastSeenPos = null;
				foreach (var step in path)
				{
					if (aiVisible.Contains(step))
						lastSeenPos = step;
				}

				if (!lastSeenPos.HasValue) continue;

				board.UpdateKnownEnemy(movingPlayer.id, lastSeenPos.Value, currentTurn, aiUnit.id);
			}
		}
	}
}

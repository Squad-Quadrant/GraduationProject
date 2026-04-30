using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Data.Runtime.Events.AI;
using Systems.Unit;
using UnityEngine;

namespace Systems.AI.Blackboard
{
	public class AIBlackboard
	{
		private readonly EUnitFaction _faction;
		private readonly IEventBus _eventBus;

		// 这里的enemy指的是和ai阵营不同的单位，即玩家
		private readonly Dictionary<string, KnownEnemyInfo> _knownEnemies = new(); // enemyId -> knownEnemyInfo
		private readonly List<ThreatRecord> _recentThreats = new();

		public IReadOnlyDictionary<string, KnownEnemyInfo> KnownEnemies => _knownEnemies;
		public IReadOnlyList<ThreatRecord> RecentThreats => _recentThreats;

		internal AIBlackboard(EUnitFaction faction, IEventBus eventBus)
		{
			_faction = faction;
			_eventBus = eventBus;
		}

		internal void UpdateKnownEnemy(string enemyId, Vector2Int pos, int turn, string seenById)
		{
			if (_knownEnemies.TryGetValue(enemyId, out var existing))
			{
				existing.LastKnownPos = pos;
				existing.LastSeenTurn = turn;
				existing.LastSeenByMemberId = seenById;
			}
			else
				_knownEnemies[enemyId] = new KnownEnemyInfo(enemyId, pos, turn, seenById);

			PublishUpdate();
		}

		internal bool RemoveKnownEnemy(string enemyId)
		{
			if (!_knownEnemies.Remove(enemyId)) return false;
			PublishUpdate();
			return true;
		}

		internal void RecordThreat(ThreatRecord record)
		{
			if (_recentThreats.Any(existing =>
					    existing.DamagedUnitId == record.DamagedUnitId && existing.Turn == record.Turn))
				return;
			_recentThreats.Add(record);
			PublishUpdate();
		}

		internal int PurgeExpiredThreats(int currentTurn, int maxAgeTurns)
		{
			int removed = _recentThreats.RemoveAll(r => currentTurn - r.Turn > maxAgeTurns);
			if (removed > 0) PublishUpdate();
			return removed;
		}

		internal int PurgeExpiredKnownEnemies(int currentTurn, int maxAgeTurns)
		{
			List<string> toRemove = null;
			foreach (var pair in _knownEnemies.Where(pair => currentTurn - pair.Value.LastSeenTurn > maxAgeTurns))
			{
				toRemove ??= new List<string>();
				toRemove.Add(pair.Key);
			}

			if (toRemove == null) return 0;
			foreach (var id in toRemove)
				_knownEnemies.Remove(id);
			PublishUpdate();
			return toRemove.Count;
		}

		private void PublishUpdate() => _eventBus.Publish(new BlackboardUpdatedEvent(_faction));
	}
}

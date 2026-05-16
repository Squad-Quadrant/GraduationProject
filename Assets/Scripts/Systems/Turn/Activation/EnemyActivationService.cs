using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Map;
using Data.Runtime.Events.Unit;
using Data.Runtime.Events.Vision;
using Systems.Map.Region;
using Systems.Unit;

namespace Systems.Turn.Activation
{
	public class EnemyActivationService : IEnemyActivationService, IDisposable
	{
		private readonly IEventBus _eventBus;
		private readonly IUnitService _unitService;
		private readonly ITurnService _turnService;
		private readonly IRegionService _regionService;

		private readonly HashSet<string> _activated = new(); // 所有已激活的敌人ID
		private readonly HashSet<string> _enqueued = new(); // 已激活且已加入TurnService 队列 的敌人ID
		private readonly Dictionary<int, List<string>> _groupIndex = new();

		public EnemyActivationService(
			IEventBus eventBus,
			IUnitService unitService,
			ITurnService turnService,
			IRegionService regionService)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
			_turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
			_regionService = regionService ?? throw new ArgumentNullException(nameof(regionService));

			_eventBus.Subscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Subscribe<UnitDestroyedEvent>(OnUnitDestroyed);
			_eventBus.Subscribe<EnemySpottedEvent>(OnEnemySpotted);
			_eventBus.Subscribe<RegionUnlockedEvent>(OnRegionUnlocked);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Unsubscribe<UnitDestroyedEvent>(OnUnitDestroyed);
			_eventBus.Unsubscribe<EnemySpottedEvent>(OnEnemySpotted);
			_eventBus.Unsubscribe<RegionUnlockedEvent>(OnRegionUnlocked);
		}
		public bool IsActivated(string unitId) => _activated.Contains(unitId);

		public IReadOnlyCollection<string> GetActivatedUnits() => _activated;

		public void Activate(string unitId)
		{
			if (_activated.Contains(unitId)) return;

			if (!_unitService.TryGetUnit(unitId, out var unit))
			{
				this.LogWarning($"Activate: unit '{unitId}' not found, skipping");
				return;
			}

			if (unit.faction != EUnitFaction.Enemy)
			{
				this.LogWarning($"Activate: '{unitId}' is not Enemy (faction={unit.faction}), skipping");
				return;
			}

			if (!unit.IsAlive) return;

			_activated.Add(unit.id);
			this.Log($"Activated enemy '{unit.id}' (group={unit.activationGroupId}, pos={unit.position})");

			TryEnqueue(unit);

			if (unit.activationGroupId <= 0 ||
			    !_groupIndex.TryGetValue(unit.activationGroupId, out var members)) return;

			var snapshot = new List<string>(members);
			foreach (var memberId in snapshot.Where(memberId => memberId != unit.id))
				Activate(memberId);
		}

		private void TryEnqueue(Unit.Unit unit)
		{
			if (_enqueued.Contains(unit.id)) return;

			if (!_regionService.IsCellUnlocked(unit.position))
			{
				this.Log($"Enemy '{unit.id}' activated but in locked region; awaiting region unlock");
				return;
			}

			_enqueued.Add(unit.id);
			_turnService.AddUnit(unit);
		}

		private void OnUnitCreated(UnitCreatedEvent e)
		{
			var unit = e.Unit;
			if (unit.faction != EUnitFaction.Enemy) return;

			var groupId = unit.activationGroupId;
			if (groupId <= 0) return; // 独立激活的敌人无需进组索引

			if (!_groupIndex.TryGetValue(groupId, out var members))
			{
				members = new List<string>();
				_groupIndex[groupId] = members;
			}
			members.Add(unit.id);
		}

		private void OnUnitDestroyed(UnitDestroyedEvent e)
		{
			var unit = e.Unit;
			if (unit.faction != EUnitFaction.Enemy) return;

			_activated.Remove(unit.id);
			_enqueued.Remove(unit.id);

			var groupId = unit.activationGroupId;
			if (groupId <= 0 || !_groupIndex.TryGetValue(groupId, out var members)) return;

			members.Remove(unit.id);
			if (members.Count == 0) _groupIndex.Remove(groupId);
		}

		private void OnEnemySpotted(EnemySpottedEvent e) => Activate(e.UnitId);

		private void OnRegionUnlocked(RegionUnlockedEvent e)
		{
			var snapshot = new List<string>(_activated);
			foreach (var unitId in snapshot.Where(unitId => !_enqueued.Contains(unitId)))
			{
				if (!_unitService.TryGetUnit(unitId, out var unit)) continue;
				if (!unit.IsAlive) continue;
				if (!_regionService.IsCellUnlocked(unit.position)) continue;

				_enqueued.Add(unitId);
				_turnService.AddUnit(unit);
				this.Log($"Region unlocked → enqueued previously-activated enemy '{unitId}'");
			}
		}
	}
}

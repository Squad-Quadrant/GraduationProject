using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Data.Runtime.Events;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.Unit;
using Systems.Unit;
using UnityEngine;

namespace Systems.Turn
{
	public class TurnService : ITurnService
	{
		private readonly IUnitService _unitService;
		private readonly IEventBus _eventBus;
		private readonly TurnData _data;

		public TurnService(IUnitService unitService, IEventBus eventBus)
		{
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_data = new TurnData();
			_eventBus.Subscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Subscribe<UnitDestroyedEvent>(OnUnitDestroyed);
			Debug.Log("[TurnService] Initialized");
		}

		private void OnUnitCreated(UnitCreatedEvent e)
		{
			ITurnUnit unit = e.Unit;
			if (unit == null) return; // Not a turn unit
			RegisterUnit(unit);
		}

		private void OnUnitDestroyed(UnitDestroyedEvent e) => UnregisterUnit(e.Unit.Id);

		public void StartTurn()
		{
			if (_data.IsTurnActive)
			{
				Debug.LogWarning("[TurnService] Cannot start turn: A turn is already active!");
				return;
			}

			if (_unitService.GetAllAliveUnits().Count == 0)
			{
				Debug.LogWarning("[TurnService] Cannot start turn: No alive units!");
				return;
			}

			_data.CurrentTurnNumber++;
			_data.IsTurnActive = true;

			Debug.Log($"[TurnService] Turn {_data.CurrentTurnNumber} Started");

			RebuildQueue();

			_eventBus.Publish(new TurnStartedEvent(_data.CurrentTurnNumber));

			NextUnit();
		}

		private void RebuildQueue()
		{
			_data.ActionQueue.Clear();

			var aliveUnits = _unitService.GetAllAliveUnits();
			var actionableUnits = aliveUnits
				.Where(u => (u as ITurnUnit).CanAct)
				.Cast<ITurnUnit>()
				.ToList();

			if (actionableUnits.Count == 0)
			{
				Debug.LogWarning("[TurnService] No actionable units to build queue!");
				return;
			}

			foreach (var unit in actionableUnits)
				unit.ActionPriority = 0;

			_data.ActionQueue.EnqueueRange(actionableUnits);

			Debug.Log($"[TurnService] Queue built with {actionableUnits.Count} units: {_data.ActionQueue}");

			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.TurnReset));
		}

		public void EndTurn()
		{
			if (!_data.IsTurnActive)
			{
				Debug.LogWarning("[TurnService] Cannot end turn: No active turn!");
				return;
			}

			if (_data.HasActingUnit) EndUnitTurn(); // End current unit's turn if active

			Debug.Log($"[TurnService] Turn {_data.CurrentTurnNumber} Ended");

			_eventBus.Publish(new TurnEndedEvent(_data.CurrentTurnNumber));

			_data.ActionQueue.Clear();
			_data.IsTurnActive = false;
		}

		public int GetCurrentTurnNumber() => _data.CurrentTurnNumber;

		public bool IsTurnActive() => _data.IsTurnActive;

		public ITurnUnit NextUnit()
		{
			if (!_data.IsTurnActive)
			{
				Debug.LogWarning("[TurnService] Cannot get next unit: No active turn!");
				return null;
			}

			// check if the queue is empty
			if (_data.ActionQueue.IsEmpty)
			{
				Debug.Log("[TurnService] No more units in queue, turn should end");
				return null;
			}

			var nextUnit = _data.ActionQueue.Dequeue(); // Get the next unit and remove it from the queue

			if (!_unitService.HasUnit(nextUnit.Id))
			{
				Debug.LogWarning($"[TurnService] Next unit '{nextUnit.Id}' not found in UnitService, skipping turn");
				return NextUnit(); // Skip to the next unit
			}

			if (!nextUnit.CanAct)
			{
				Debug.Log($"[TurnService] Next unit '{nextUnit.Id}' cannot act, skipping turn");
				return NextUnit(); // Skip to the next unit
			}

			_data.CurrentActingUnit = nextUnit;
			Debug.Log($"[TurnService] Unit '{nextUnit.Id}' turn started (Speed: {nextUnit.Speed})");
			_eventBus.Publish(new UnitTurnStartedEvent(nextUnit.Id, _data.CurrentTurnNumber));
			return nextUnit;
		}

		public void EndUnitTurn()
		{
			if (!_data.HasActingUnit)
			{
				Debug.LogWarning("[TurnService] Cannot end unit turn: No active unit!");
				return;
			}

			var unitId = _data.CurrentActingUnit.Id;
			Debug.Log($"[TurnService] Unit '{unitId}' turn ended");
			_eventBus.Publish(new UnitTurnEndedEvent(unitId, _data.CurrentTurnNumber));
			_data.CurrentActingUnit = null;
		}

		public ITurnUnit GetCurrentUnit() => _data.CurrentActingUnit;

		public bool HasCurrentUnit() => _data.HasActingUnit;

		public void RegisterUnit(ITurnUnit unit)
		{
			if (unit == null)
				throw new ArgumentNullException(nameof(unit));

			if (!unit.CanAct)
			{
				Debug.LogWarning($"[TurnService] Cannot register unit '{unit.Id}': Unit cannot act.");
				return;
			}

			if (_data.IsTurnActive)
			{
				_data.ActionQueue.Enqueue(unit);
				Debug.Log($"[TurnService] Registered unit '{unit.Id}' into active turn queue.");
				_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.UnitAdded, unit.Id));
			}
			else
			{
				// todo: ?
				Debug.Log($"[TurnService] Unit '{unit.Id}' registered, will join queue on next StartTurn()");
			}
		}

		public void UnregisterUnit(string unitId)
		{
			bool removed = _data.ActionQueue.Remove(unitId);

			if (_data.HasActingUnit && _data.CurrentActingUnit.Id == unitId)
			{
				Debug.Log($"[TurnService] Current acting unit '{unitId}' was unregistered, clearing reference");
				_data.CurrentActingUnit = null;
			}

			if (!removed) return;
			Debug.Log($"[TurnService] Unit '{unitId}' unregistered");
			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.UnitRemoved, unitId));
		}

		public bool IsUnitRegistered(string unitId) => _data.ActionQueue.Contains(unitId);

		public void UpdateUnitSpeed(string unitId)
		{
			if (!_data.ActionQueue.Contains(unitId))
			{
				Debug.LogWarning($"[TurnService] Unit '{unitId}' not in queue, cannot update speed");
				return;
			}

			_data.ActionQueue.Reorder();
			Debug.Log($"[TurnService] Unit '{unitId}' updated speed, queue reordered");
			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.SpeedModified, unitId));
		}

		public void ForceInsertUnit(string unitId, int priority = int.MaxValue)
		{
			if (!_data.ActionQueue.Contains(unitId))
			{
				Debug.LogWarning($"[TurnService] Unit '{unitId}' not in queue, cannot force insert");
				return;
			}

			_data.ActionQueue.InsertWithPriority(unitId, priority);

			Debug.Log($"[TurnService] Unit '{unitId}' force inserted with priority {priority}");
			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.UnitInserted, unitId));
		}

		public IReadOnlyList<ITurnUnit> GetTurnOrder() => _data.ActionQueue.GetQueue();

		public int GetUnitPositionInQueue(string unitId) => _data.ActionQueue.GetPositionInQueue(unitId);

		public int GetRemainingUnitsCount() => _data.ActionQueue.Count;

		public bool IsCurrentTurnComplete() => _data.IsTurnActive && _data.ActionQueue.IsEmpty && !_data.HasActingUnit;

		public void Clear()
		{
			Debug.Log("[TurnService] Clearing...");
			_data.Reset();
			Debug.Log("[TurnService] Cleared");
		}
	}
}

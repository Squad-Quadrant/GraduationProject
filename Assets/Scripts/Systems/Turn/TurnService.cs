using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.Unit;
using Systems.Unit;

namespace Systems.Turn
{
	public class TurnService : ITurnService, IDisposable
	{
		private readonly IUnitService _unitService;
		private readonly IEventBus _eventBus;
		private readonly TurnData _data = new();

		public TurnService(IUnitService unitService, IEventBus eventBus)
		{
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

			_eventBus.Subscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Subscribe<UnitDestroyedEvent>(OnUnitDestroyed);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<UnitCreatedEvent>(OnUnitCreated);
			_eventBus.Unsubscribe<UnitDestroyedEvent>(OnUnitDestroyed);
		}

		private void OnUnitCreated(UnitCreatedEvent e)
		{
			if (e.Unit is not ITurnUnit turnUnit) return;
			if (turnUnit.Faction == EUnitFaction.Enemy) return;

			AddUnit(turnUnit);
		}

		private void OnUnitDestroyed(UnitDestroyedEvent e) => RemoveUnit(e.Unit.id);

		#region Turn Lifecycle

		public int TurnNumber => _data.TurnNumber;

		public bool IsTurnActive => _data.IsTurnActive;

		public void StartTurn()
		{
			if (_data.IsTurnActive)
			{
				this.LogWarning("Cannot start turn: already active");
				return;
			}

			var activeUnits = new List<ITurnUnit>(_data.RegisteredUnits.Count);
			foreach (var unit in _data.RegisteredUnits.Values)
			{
				if (!_unitService.HasUnit(unit.Id)) continue;
				activeUnits.Add(unit);
			}

			if (activeUnits.Count == 0)
			{
				this.LogWarning("Cannot start turn: no actionable units");
				return;
			}

			_data.TurnNumber++;
			_data.IsTurnActive = true;

			foreach (var unit in activeUnits)
				unit.ActionPriority = 0;

			_data.Queue.Build(activeUnits);

			this.Log($"Turn {_data.TurnNumber} started, {activeUnits.Count} units queued");

			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.TurnReset));
			_eventBus.Publish(new TurnStartedEvent(_data.TurnNumber));
		}

		public void EndTurn()
		{
			if (!_data.IsTurnActive)
			{
				this.LogWarning("Cannot end turn: no active turn");
				return;
			}

			if (_data.IsUnitActing) // 强制结束当前单位回合
				EndUnitTurn();

			_data.Queue.Clear();
			_data.IsTurnActive = false;

			this.Log($"Turn {_data.TurnNumber} ended");

			_eventBus.Publish(new TurnEndedEvent(_data.TurnNumber));
		}

		#endregion

		#region UnitTurn Lifecycle

		public ITurnUnit ActiveUnit => _data.ActiveUnit;

		public bool IsUnitActing => _data.IsUnitActing;

		public bool IsTurnComplete => _data.IsTurnActive && _data.Queue.IsExhausted && !_data.IsUnitActing;

		public ITurnUnit NextUnit()
		{
			if (!_data.IsTurnActive)
			{
				this.LogWarning("Cannot advance: no active turn");
				return null;
			}

			if (_data.IsUnitActing)
			{
				this.LogWarning($"Cannot advance: unit '{_data.ActiveUnit.Id}' is still acting. Call EndUnitTurn() first.");
				return null;
			}

			while (true)
			{
				var next = _data.Queue.Advance();

				if (next == null) // queue exhausted
				{
					this.Log("No more units to act this turn");
					return null;
				}

				if (!_unitService.HasUnit(next.Id))
				{
					this.LogWarning($"Unit '{next.Id}' no longer exists, skipping");
					continue;
				}

				if (!next.CanAct)
				{
					this.Log($"Unit '{next.Id}' cannot act, skipping");
					continue;
				}

				_data.IsUnitActing = true; // 找到了一个可行动的单位，开始它的回合
				next.OnTurnStart();

				this.Log($"Unit '{next.Id}' turn started (Speed:{next.Speed})");
				this.Log($"单位 '{next.Id}' 回合开始", true);

				_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.UnitAdvanced, next.Id));

				return next;
			}
		}

		public void EndUnitTurn()
		{
			if (!_data.IsUnitActing)
			{
				this.LogWarning("Cannot end unit turn: no unit is acting");
				return;
			}

			var unitId = _data.ActiveUnit.Id;
			_data.IsUnitActing = false;

			this.Log($"Unit '{unitId}' turn ended");

			_eventBus.Publish(new UnitTurnEndedEvent(unitId, _data.TurnNumber));
		}

		#endregion

		#region Unit Order Management

		// 添加一个单位到队列中，并重新排序可行动单位
		public void AddUnit(ITurnUnit unit)
		{
			if (unit == null)
			{
				this.LogError("Cannot add null unit");
				return;
			}

			if (!_data.RegisteredUnits.TryAdd(unit.Id, unit))
			{
				this.LogWarning($"Unit '{unit.Id}' is already registered, skipping");
				return;
			}

			if (!_data.IsTurnActive)
			{
				this.Log($"Unit '{unit.Id}' will join queue on next turn start");
				return;
			}

			_data.Queue.Add(unit);
			this.Log($"Added unit '{unit.Id}' to upcoming queue");
			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.UnitAdded, unit.Id));
		}

		public void RemoveUnit(string unitId)
		{
			bool wasRegistered = _data.RegisteredUnits.Remove(unitId);

			if (_data.IsUnitActing && _data.ActiveUnit?.Id == unitId)
			{
				this.Log($"Removing currently acting unit '{unitId}', ending their turn");
				EndUnitTurn();
			}

			bool wasInQueue = _data.Queue.Remove(unitId);

			if (!wasRegistered && !wasInQueue) return;

			this.Log($"Removed unit '{unitId}' (registered:{wasRegistered}, inQueue:{wasInQueue})");
			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.UnitRemoved, unitId));
		}

		// 设置一个单位的优先级，即刻重新排序可行动单位，但不会影响已经行动过的单位
		public void SetUnitPriority(string unitId, int priority)
		{
			if (!_data.Queue.SetPriority(unitId, priority))
				return;

			this.Log($"Set unit '{unitId}' priority to {priority}");
			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.PriorityChanged, unitId));
		}

		// 强制让一个单位在下一回合立刻行动，和SetPriority不同的是其会完全忽略Speed的影响
		// 实际上就是把它的ActionPriority设置成当前所有单位中最高的那个+1
		public void MoveUnitToNext(string unitId)
		{
			if (!_data.Queue.MoveToNext(unitId))
				return;

			this.Log($"Moved unit '{unitId}' to act next");
			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.PriorityChanged, unitId));
		}

		public void ResortQueue()
		{
			_data.Queue.ResortUpcoming();

			this.Log("Queue re-sorted after speed changes");
			_eventBus.Publish(new TurnOrderChangedEvent(TurnOrderChangeReason.SpeedChanged));
		}

		public void Clear()
		{
			this.Log("Clearing all turn state");
			_data.Reset();
		}

		#endregion

		#region Query

		public IReadOnlyList<ITurnUnit> GetFullOrder() => _data.Queue.GetFullOrder();

		public IReadOnlyList<ITurnUnit> GetUpcoming() => _data.Queue.GetUpcoming();

		public int IndexOf(string unitId) => _data.Queue.IndexOf(unitId);

		#endregion
	}
}

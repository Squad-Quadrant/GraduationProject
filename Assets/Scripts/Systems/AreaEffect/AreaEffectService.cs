using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.AreaEffect;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.Unit;
using Systems.Buff;
using Systems.Damage;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;

namespace Systems.AreaEffect
{
	public class AreaEffectService : IAreaEffectService, IDisposable
	{
		private readonly IEventBus _eventBus;
		private readonly AreaEffectContext _ctx;

		private readonly Dictionary<string, AreaEffect> _effects = new(); // effectId → AreaEffect
		private readonly Dictionary<Vector2Int, List<AreaEffect>> _cellIndex = new(); // cell → List<AreaEffect>，同一格可能有多个 effect
		private int _idCounter = -1;

		public AreaEffectService(
			IEventBus eventBus,
			IUnitService unitService,
			IDamageService damageService,
			IVisionService visionService,
			IVisionCalculator visionCalculator,
			IBuffService buffService)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_ctx = new AreaEffectContext(eventBus, unitService, damageService, visionService, visionCalculator, buffService);

			_eventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
			_eventBus.Subscribe<UnitTurnStartedEvent>(OnUnitTurnStarted);
			_eventBus.Subscribe<UnitMovedEvent>(OnUnitMoved);
			_eventBus.Subscribe<UnitDestroyedEvent>(OnUnitDestroyed);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
			_eventBus.Unsubscribe<UnitTurnStartedEvent>(OnUnitTurnStarted);
			_eventBus.Unsubscribe<UnitMovedEvent>(OnUnitMoved);
			_eventBus.Unsubscribe<UnitDestroyedEvent>(OnUnitDestroyed);

			foreach (var id in new List<string>(_effects.Keys))
				Unregister(id);

			this.Log("Disposed");
		}

		public IReadOnlyCollection<AreaEffect> GetAll() => _effects.Values;

		public IReadOnlyList<AreaEffect> GetAt(Vector2Int cell) =>
			_cellIndex.TryGetValue(cell, out var list) ? list : Array.Empty<AreaEffect>();

		public bool TryGet(string effectId, out AreaEffect effect) =>
			_effects.TryGetValue(effectId, out effect);

		public AreaEffect Register(
			string ownerId,
			Vector2Int targetCell,
			IReadOnlyList<Vector2Int> cells,
			int remainingTurns,
			AreaEffectBehavior behavior)
		{
			if (cells == null || cells.Count == 0)
				throw new ArgumentException("cells must be non-empty", nameof(cells));
			if (behavior == null)
				throw new ArgumentNullException(nameof(behavior));

			if (cells.All(c => c != targetCell))
				throw new ArgumentException($"targetCell {targetCell} must be one of cells", nameof(targetCell));

			_idCounter++;
			var id = $"ae_{_idCounter}_{ownerId ?? "anonymous"}";
			var effect = new AreaEffect(id, ownerId, targetCell, cells, remainingTurns, behavior);

			_effects[id] = effect;
			foreach (var cell in cells)
			{
				if (!_cellIndex.TryGetValue(cell, out var list))
				{
					list = new List<AreaEffect>();
					_cellIndex[cell] = list;
				}
				list.Add(effect);
			}

			behavior.OnCreated(effect, _ctx);
			_eventBus.Publish(new AreaEffectRegisteredEvent(effect));

			foreach (var cell in cells)
            {
                var unit = _ctx.UnitService.GetUnitAtPosition(cell);
				if (unit != null)
				{
					behavior.OnUnitEntered(effect, unit, cell, _ctx);
				}
			}

			this.Log($"Registered: {effect}");
			return effect;
		}

		public void Unregister(string effectId)
		{
			if (!_effects.TryGetValue(effectId, out var effect))
			{
				this.LogWarning($"Unregister: effectId '{effectId}' not found");
				return;
			}

			effect.Behavior.OnRemoved(effect, _ctx);

			_effects.Remove(effectId);
			foreach (var cell in effect.Cells)
			{
				if (!_cellIndex.TryGetValue(cell, out var list)) continue;
				list.Remove(effect);
				if (list.Count == 0) _cellIndex.Remove(cell);
			}

			_eventBus.Publish(new AreaEffectUnregisteredEvent(effect.Id, effect.Cells));
			this.Log($"Unregistered: {effect.Id}");
		}

		// 大回合开始 → 所有 effect 的 RemainingTurns 统一 --
		private void OnTurnStarted(TurnStartedEvent e)
		{
			var snapshot = new List<AreaEffect>(_effects.Values);
			foreach (var effect in snapshot)
			{
				effect.RemainingTurns--;
				if (effect.RemainingTurns >= 0) continue;

				effect.Behavior.OnExpired(effect, _ctx);
				if (_effects.ContainsKey(effect.Id))
					Unregister(effect.Id);
			}
		}

		// 某单位开启自己的回合 → 若站在某 effect 覆盖格内，触发 OnUnitTurnStartInside
		private void OnUnitTurnStarted(UnitTurnStartedEvent e)
		{
			if (!_cellIndex.TryGetValue(e.CellPosition, out var effectsAtCell)) return;
			if (!_ctx.UnitService.TryGetUnit(e.TurnUnitId, out var unit)) return;

			var snapshot = new List<AreaEffect>(effectsAtCell);
			foreach (var effect in snapshot)
				effect.Behavior.OnUnitTurnStart(effect, unit, e.CellPosition, _ctx);
		}

		// 某单位完成一次移动 → Path 每一格（排除起点）触发 OnUnitEntered
		private void OnUnitMoved(UnitMovedEvent e)
		{
			if (e.Path == null || e.Path.Count < 2) return;

			var unit = e.Unit;
			
			var activeEffects = new HashSet<AreaEffect>();
			if (_cellIndex.TryGetValue(e.Path[0], out var startList))
			{
				activeEffects.UnionWith(startList);
			}

			for (int i = 1; i < e.Path.Count; i++)
			{
				var currentCell = e.Path[i];
				var currentEffects = new HashSet<AreaEffect>();
				if (_cellIndex.TryGetValue(currentCell, out var currentList))
				{
					currentEffects.UnionWith(currentList);
				}

				foreach (var effect in activeEffects.Except(currentEffects).ToList())
				{
					effect.Behavior.OnUnitLeft(effect, unit, e.Path[i - 1], _ctx);
				}

				foreach (var effect in currentEffects.Except(activeEffects).ToList())
				{
					effect.Behavior.OnUnitEntered(effect, unit, currentCell, _ctx);
				}

				activeEffects = currentEffects;
			}
		}

		// 某单位死亡 → 清除所有 owner=该单位 且 DestroyOnOwnerDeath=true 的 effect
		private void OnUnitDestroyed(UnitDestroyedEvent e)
		{
			if (e.Unit == null) return;
			var deadUnitId = e.Unit.id;

			List<string> toRemove = _effects.Values
				.Where(effect => effect.OwnerId == deadUnitId)
				.Where(effect => effect.Behavior.DestroyOnOwnerDeath)
				.Select(effect => effect.Id)
				.ToList();

			if (toRemove.Count <= 0) return;
			foreach (var id in toRemove)
				Unregister(id);
		}
	}
}

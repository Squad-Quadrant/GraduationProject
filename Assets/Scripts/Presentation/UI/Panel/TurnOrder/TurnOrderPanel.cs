using System.Collections.Generic;
using Core.Log;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Systems.Unit;
using UnityEngine;

namespace Presentation.UI.Panel.TurnOrder
{
	public enum ESlotState
	{
		Acted, Current, Upcoming
	}

	// TurnOrder接收到的Unit会是ITurnUnit的，只有UnitId，Icon和FactionColor需要手动从UnitService中查询，所以这里打包一个数据结构，方便TurnOrderPanel使用
	public readonly struct SlotData
	{
		public readonly string UnitId;
		public readonly Sprite Icon;
		public readonly Sprite FactionBg;
		public readonly ESlotState State;

		public SlotData(string unitId, Sprite icon, Sprite factionBg, ESlotState state)
		{
			UnitId = unitId;
			Icon = icon;
			FactionBg = factionBg;
			State = state;
		}
	}

	public class TurnOrderPanel : UIPanel
	{
		#region Config

		[TitleGroup("References")]
		[SerializeField, Required] private TurnOrderSlot slotPrefab;
		[SerializeField, Required] private RectTransform slotContainer;

		[TitleGroup("Layout")]
		[SerializeField] private float slotWidth = 80f;
		[SerializeField] private float spacing = 8f; // space between slots in pixels
		[SerializeField] private int maxVisibleSlots = 15;

		[TitleGroup("Visual States")]
		[SerializeField] private float highlightScale = 1.25f;
		[SerializeField] private float actedAlpha = 0.4f;

		[TitleGroup("Faction Bg")]
		[SerializeField, Required] private Sprite playerBg;
		[SerializeField, Required] private Sprite enemyBg;

		[TitleGroup("Animation")]
		[SerializeField, Min(0.01f)] private float stateDuration = 0.25f;
		[SerializeField, Min(0.01f)] private float slideDuration = 0.3f;
		[SerializeField, Min(0.01f)] private float entranceDuration = 0.35f;
		[SerializeField, Min(0f)] private float entranceStagger = 0.05f;
		[SerializeField, Min(0.01f)] private float exitDuration = 0.25f;

		#endregion

		private readonly List<TurnOrderSlot> _activeSlots = new();
		private readonly Dictionary<string, TurnOrderSlot> _slotMap = new();
		private readonly Queue<TurnOrderSlot> _pool = new();

		private string _currentUnitId;

		protected override void OnClose()
		{
			ReturnAllSlots();
			_currentUnitId = null;
			this.Log("Closed, all slots returned to pool");
		}

		public void Rebuild(SlotData[] slots) // 重建整个回合顺序UI，通常在回合开始时调用
		{
			ReturnAllSlots();
			_currentUnitId = null;

			int count = Mathf.Min(slots.Length, maxVisibleSlots);
			for (int i = 0; i < count; i++)
			{
				var data = slots[i];

				var slot = AcquireSlot();
				slot.Setup(data.UnitId, data.Icon, data.FactionBg);

				float x = CalculateSlotX(i, count);
				slot.SetX(x);

				var (scale, alpha) = GetStateVisuals(data.State);
				slot.SetState(scale, alpha);

				if (data.State == ESlotState.Current)
					_currentUnitId = data.UnitId;

				_activeSlots.Add(slot);
				_slotMap[data.UnitId] = slot;

				slot.AnimateEntrance(0f, entranceDuration, i * entranceStagger);
			}

			this.Log($"Rebuilt with {count} slots" +
			         (_currentUnitId != null ? $", current: {_currentUnitId}" : ""));
		}

		public void AdvanceTo(string newCurrentUnitId) // 推进回合时
		{
			if (string.IsNullOrEmpty(newCurrentUnitId))
			{
				this.LogWarning("AdvanceTo called with null/empty unitId");
				return;
			}

			int newIndex = _activeSlots.FindIndex(s => s.UnitId == newCurrentUnitId);
			if (newIndex < 0)
			{
				this.LogWarning($"AdvanceTo: slot not found for '{newCurrentUnitId}'");
				return;
			}

			int oldIndex = string.IsNullOrEmpty(_currentUnitId)
				? -1
				: _activeSlots.FindIndex(s => s.UnitId == _currentUnitId);

			if (oldIndex >= 0)
				ApplyState(_activeSlots[oldIndex], ESlotState.Acted, animate: true);

			int dimStart = Mathf.Max(0, oldIndex + 1);
			for (int i = dimStart; i < newIndex; i++)
				ApplyState(_activeSlots[i], ESlotState.Acted, animate: true);

			ApplyState(_activeSlots[newIndex], ESlotState.Current, animate: true);
			_currentUnitId = newCurrentUnitId;
			this.Log($"Advanced to '{newCurrentUnitId}' (index {newIndex})");
		}

		public void RemoveSlot(string unitId) // 当一个单位死亡或被移除时
		{
			if (!_slotMap.TryGetValue(unitId, out var slot))
			{
				this.LogWarning($"RemoveSlot: no slot found for '{unitId}'");
				return;
			}

			if (_currentUnitId == unitId)
				_currentUnitId = null;

			_activeSlots.Remove(slot);
			_slotMap.Remove(unitId);

			slot.AnimateExit(exitDuration, () =>
			{
				ReturnSlot(slot);
				RecalculatePositions(animate: true);
			});

			this.Log($"Removing slot for '{unitId}', {_activeSlots.Count} remaining");
		}

		public Sprite GetFactionBg(EUnitFaction faction) => faction switch
		{
			EUnitFaction.Player  => playerBg,
			EUnitFaction.Enemy   => enemyBg,
			EUnitFaction.Neutral => playerBg,
			_                   => playerBg
		};

		private (float scale, float alpha) GetStateVisuals(ESlotState state) => state switch
		{
			ESlotState.Acted   => (1f, actedAlpha),
			ESlotState.Current => (highlightScale, 1f),
			ESlotState.Upcoming => (1f, 1f),
			_ => (1f, 1f)
		};

		private void ApplyState(TurnOrderSlot slot, ESlotState state, bool animate)
		{
			var (scale, alpha) = GetStateVisuals(state);
			if (animate)
				slot.AnimateState(scale, alpha, stateDuration);
			else
				slot.SetState(scale, alpha);
		}

		private float CalculateSlotX(int index, int totalCount)
		{
			float totalWidth = totalCount * slotWidth + (totalCount - 1) * spacing;
			float startX = -totalWidth / 2f + slotWidth / 2f;
			return startX + index * (slotWidth + spacing);
		}

		private void RecalculatePositions(bool animate)
		{
			int count = _activeSlots.Count;
			for (int i = 0; i < count; i++)
			{
				float targetX = CalculateSlotX(i, count);
				if (animate)
					_activeSlots[i].AnimateToX(targetX, slideDuration);
				else
					_activeSlots[i].SetX(targetX);
			}
		}

		#region Object Pool

		private TurnOrderSlot AcquireSlot()
		{
			TurnOrderSlot slot;

			slot = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(slotPrefab, slotContainer);

			slot.gameObject.SetActive(true);
			return slot;
		}

		private void ReturnSlot(TurnOrderSlot slot)
		{
			if (!slot) return;
			slot.ResetForPool();
			slot.gameObject.SetActive(false);
			_pool.Enqueue(slot);
		}

		private void ReturnAllSlots()
		{
			foreach (var slot in _activeSlots)
				ReturnSlot(slot);
			_activeSlots.Clear();
			_slotMap.Clear();
		}

		#endregion
	}
}

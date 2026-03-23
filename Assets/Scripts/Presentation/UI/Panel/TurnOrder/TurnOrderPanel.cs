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

	public readonly struct SlotVisual
	{
		public readonly float Scale;
		public readonly float Alpha;
		public readonly float OffsetY;

		public SlotVisual(float scale, float alpha, float offsetY)
		{
			Scale = scale;
			Alpha = alpha;
			OffsetY = offsetY;
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
		[SerializeField] private float currentScale = 1f;
		[SerializeField] private float currentOffsetY = -12f;
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
		private readonly HashSet<string> _newIdSet = new();

		protected override void OnClose()
		{
			ReturnAllSlots();
			this.Log("Closed, all slots returned to pool");
		}

		public void Refresh(SlotData[] newSlots)
		{
			int count = Mathf.Min(newSlots.Length, maxVisibleSlots);

			_newIdSet.Clear();
			for (int i = 0; i < count; i++)
				_newIdSet.Add(newSlots[i].UnitId);

			for (int i = _activeSlots.Count - 1; i >= 0; i--)
			{
				var slot = _activeSlots[i];
				if (_newIdSet.Contains(slot.UnitId)) continue;

				_activeSlots.RemoveAt(i);
				_slotMap.Remove(slot.UnitId);
				slot.PlayExit(exitDuration, onComplete: () => ReturnSlot(slot));
			}
			_activeSlots.Clear();

			int addedCount = 0;
			for (int i = 0; i < count; i++)
			{
				var data = newSlots[i];
				var visual = GetVisual(data.State);
				float x = CalculateSlotX(i, count);

				if (_slotMap.TryGetValue(data.UnitId, out var existing))
				{
					existing.AnimateToX(x, slideDuration);
					existing.AnimateVisual(visual, stateDuration);
					_activeSlots.Add(existing);
				}
				else
				{
					var slot = AcquireSlot();
					slot.Setup(data.UnitId, data.Icon, data.FactionBg);
					slot.SetX(x);
					slot.PlayEntrance(visual, entranceDuration, delay: addedCount * entranceStagger);

					_activeSlots.Add(slot);
					_slotMap[data.UnitId] = slot;
					addedCount++;
				}
			}

			_newIdSet.Clear();
			this.Log($"Refreshed: {count} slots, {addedCount} added");
		}

		public Sprite GetFactionBg(EUnitFaction faction) => faction switch
		{
			EUnitFaction.Enemy => enemyBg,
			_ => playerBg
		};

		private SlotVisual GetVisual(ESlotState state) => state switch
		{
			ESlotState.Current  => new SlotVisual(currentScale, 1f, currentOffsetY),
			ESlotState.Acted    => new SlotVisual(1f, actedAlpha, 0f),
			ESlotState.Upcoming => new SlotVisual(1f, 1f, 0f),
			_                   => new SlotVisual(1f, 1f, 0f)
		};

		private float CalculateSlotX(int index, int totalCount)
		{
			float stride = slotWidth + spacing;
			float totalWidth = totalCount * slotWidth + (totalCount - 1) * spacing;
			float startX = -totalWidth / 2f + slotWidth / 2f;
			return startX + index * stride;
		}

		#region Object Pool

		private TurnOrderSlot AcquireSlot()
		{
			var slot = _pool.Count > 0
				? _pool.Dequeue()
				: Instantiate(slotPrefab, slotContainer);
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

using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Turn;
using Presentation.UI.Core;
using Presentation.UI.Panel.TurnOrder;
using Systems.Turn;
using Systems.Unit;
using UnityEngine;

namespace Presentation.UI.Presenter
{
	public class TurnOrderPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;
		private readonly ITurnService _turnService;
		private readonly IUnitService _unitService;

		private TurnOrderPanel _panel;

		public TurnOrderPresenter(
			UIManager uiManager,
			IEventBus eventBus,
			ITurnService turnService,
			IUnitService unitService)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));

			_eventBus.Subscribe<TurnOrderChangedEvent>(OnTurnOrderChanged);

			this.Log("Initialized");
		}
		public void Dispose()
		{
			_eventBus.Unsubscribe<TurnOrderChangedEvent>(OnTurnOrderChanged);

			if (_panel)
			{
				_uiManager.Close<TurnOrderPanel>();
				_panel = null;
			}

			this.Log("Disposed");
		}

		private void OnTurnOrderChanged(TurnOrderChangedEvent e)
		{
			if (!_panel) _panel = _uiManager.Open<TurnOrderPanel>();
			if (!_panel)
			{
				this.LogError("Failed to open TurnOrderPanel");
				return;
			}

			switch (e.Reason)
			{
				case TurnOrderChangeReason.TurnReset:
				case TurnOrderChangeReason.UnitAdded:
				case TurnOrderChangeReason.PriorityChanged:
				case TurnOrderChangeReason.SpeedChanged:
					var slots = BuildSlotData();
					_panel.Rebuild(slots);
					this.Log($"QueueChanged: Rebuilt with {slots.Length} slots");
					break;

				case TurnOrderChangeReason.UnitAdvanced:
					_panel.AdvanceTo(e.AffectedUnitId);
					this.Log($"UnitAdvanced: '{e.AffectedUnitId}'");
					break;

				case TurnOrderChangeReason.UnitRemoved:
					_panel.RemoveSlot(e.AffectedUnitId);
					this.Log($"UnitRemoved: '{e.AffectedUnitId}'");
					break;

				default:
					this.LogWarning($"Unhandled reason: {e.Reason}");
					break;
			}
		}

		private SlotData[] BuildSlotData()
		{
			var order = _turnService.GetFullOrder();
			if (order == null || order.Count == 0)
				return Array.Empty<SlotData>();

			int activeIndex = -1;
			var activeUnit = _turnService.ActiveUnit;
			if (activeUnit != null)
			{
				for (int i = 0; i < order.Count; i++)
				{
					if (order[i].Id != activeUnit.Id) continue;
					activeIndex = i;
					break;
				}
			}

			var result = new SlotData[order.Count];

			for (int i = 0; i < order.Count; i++)
			{
				var turnUnit = order[i];
				var state = ClassifyState(i, activeIndex);
				Sprite icon = null;
				var factionColor = _panel.GetFactionColor(UnitFaction.Neutral);

				if (_unitService.TryGetUnit(turnUnit.Id, out var unit))
				{
					icon = unit.icon;
					factionColor = _panel.GetFactionColor(unit.stats.faction);
				}

				result[i] = new SlotData(turnUnit.Id, icon, factionColor, state);
			}

			return result;
		}

		private static ESlotState ClassifyState(int index, int activeIndex)
		{
			if (activeIndex < 0)
				return ESlotState.Upcoming;
			if (index == activeIndex)
				return ESlotState.Current;
			return index < activeIndex
				? ESlotState.Acted
				: ESlotState.Upcoming;
		}
	}
}

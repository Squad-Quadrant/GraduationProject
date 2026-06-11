using System;
using System.Collections.Generic;
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
			this.Log($"TurnOrderChanged: {e.Reason}, affected='{e.AffectedUnitId}'");

			if (!_panel) _panel = _uiManager.Open<TurnOrderPanel>();
			if (!_panel)
			{
				this.LogError("Failed to open TurnOrderPanel");
				return;
			}

			_panel.Refresh(BuildSlotData());
		}

		private SlotData[] BuildSlotData()
		{
			var order = _turnService.GetFullOrder();
			if (order == null || order.Count == 0)
				return Array.Empty<SlotData>();

			// Find the active unit's index for state classification
			int activeIndex = FindActiveIndex(order);
			var result = new SlotData[order.Count];

			for (int i = 0; i < order.Count; i++)
			{
				var turnUnit = order[i];
				var state = ClassifyState(i, activeIndex);

				Sprite icon = null;
				var factionBg = _panel.GetFactionBg(EUnitFaction.Neutral);
				string unitName = "unit";

				if (_unitService.TryGetUnit(turnUnit.Id, out var unit))
				{
					icon = unit.icon;
					factionBg = _panel.GetFactionBg(unit.faction);
					unitName = unit.name;
				}

				result[i] = new SlotData(turnUnit.Id, icon, factionBg, unitName, state);
			}

			return result;
		}

		private int FindActiveIndex(IReadOnlyList<ITurnUnit> order)
		{
			var activeUnit = _turnService.ActiveUnit;
			if (activeUnit == null) return -1;

			for (int i = 0; i < order.Count; i++)
				if (order[i].Id == activeUnit.Id)
					return i;

			return -1;
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

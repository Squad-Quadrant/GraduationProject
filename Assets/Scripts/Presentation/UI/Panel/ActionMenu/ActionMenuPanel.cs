using System.Collections.Generic;
using System.Linq;
using Data.Runtime;
using Data.Runtime.Events.UI;
using Data.Runtime.Events.Unit;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Systems.Unit;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.ActionMenu
{
    public class ActionMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
    {
	    [SerializeField, Required, ChildGameObjectsOnly] private Image actionDescPanel;
        [SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI actionDesc;
        [SerializeField, Required, ChildGameObjectsOnly] private Transform itemsParent;

        [SerializeField] private string playerLockedMessage = "行动选择";
        [SerializeField] private string allyLockedMessage = "未到该单位的回合，无法操作";
        [SerializeField] private string enemyLockedMessage = "敌方单位，无法操作";

        [SerializeField] private Color normalColor;
        [SerializeField] private Color disabledColor;
        [SerializeField] private Color lockedColor;

        private List<ActionMenuItem> _items;

        private bool _locked;

        public void DataInitialize(Systems.Unit.Unit unit) { }

        protected override void OnOpen() => EventBus.Subscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);

        protected override void OnClose() => EventBus.Unsubscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);

        public void ShowLocked(Systems.Unit.Unit unit)
        {
	        _locked = true;
	        _items ??= itemsParent.GetComponentsInChildren<ActionMenuItem>(true).ToList();
	        foreach (var item in _items)
	        {
		        item.Button.onClick.RemoveAllListeners();
		        item.OnHoverEnter = null;
		        item.OnHoverExit = null;
		        item.Interactable = false;
		        item.SetAudioEnabled(false);
	        }
	        SetActionDesc(unit.faction == EUnitFaction.Player ? allyLockedMessage : enemyLockedMessage);
	        SetActionDescColor(lockedColor);
        }

        public void ShowActions(Systems.Unit.Unit unit)
        {
	        _locked = false;
	        _items ??= itemsParent.GetComponentsInChildren<ActionMenuItem>(true).ToList();
	        foreach (var item in _items)
	        {
		        item.Button.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(item.ActionType, item.Payload)));

		        item.OnHoverEnter += SetActionDesc;
		        item.OnHoverEnter += _ =>
		        {
			        if (item.Interactable)
				        EventBus.Publish(new ActionHoverEvent(item.ActionType, item.Payload, true));

			        SetActionDescColor(item.Interactable ? normalColor : disabledColor);
		        };

		        item.OnHoverExit += _ =>
		        {
			        SetActionDesc(playerLockedMessage);
			        SetActionDescColor(normalColor);
			        EventBus.Publish(new ActionHoverEvent(item.ActionType, item.Payload, false));
		        };

		        item.SetActive(false);
		        item.SetAudioEnabled(true);
	        }
	        Refresh(unit);
        }

        private void OnUnitInfoChanged(UnitInfoChangedEvent e)
        {
	        if (_locked) return;
	        Refresh(e.Unit);
        }

        private void Refresh(Systems.Unit.Unit unit)
        {
	        foreach (var item in _items) item.SetActive(false);
	        SetActionDesc(playerLockedMessage);

            var availableActions = unit.GetAvailableActions();
            foreach (var action in availableActions)
            {
                var item = GetItem(action.ActionType, action.Payload);
                if (!item) continue;

                item.SetActive(true);
                item.Interactable = action.IsAvailable;

                SetContent(item, unit, action);

                item.SetIconAspectRatio();
            }
        }

        private void SetActionDesc(string desc)
        {
	        actionDescPanel.gameObject.SetActive(!string.IsNullOrEmpty(desc));
	        actionDesc.text = desc;
        }

        private void SetActionDescColor(Color color)
        {
	        actionDescPanel.color = color;
	        actionDesc.color = new Color(actionDesc.color.r, actionDesc.color.g, actionDesc.color.b, color.a);
        }

        private static void SetContent(ActionMenuItem item, Systems.Unit.Unit unit, ActionAbility action)
        {
	        switch (action.ActionType)
	        {
		        case EActionType.UseTacticalItem:
		        {
			        var container = unit.GetTacticalItem(action.Payload);
			        if (container.IsNullOrEmpty()) return;
			        if (container.Logic is not TacticalItemLogic itemLogic) return;

			        item.SetContent(
				        container.Config.icon,
				        container.Config.displayName,
				        itemLogic.RemainingUses);
			        return;
		        }
		        case EActionType.UseSkill:
			        if (unit.Skill == null) return;
			        item.SetContent(
				        unit.Skill.Config.icon,
				        unit.Skill.Config.skillName,
				        unit.Skill.CurrentCooldown);
			        return;
	        }
        }

        private ActionMenuItem GetItem(EActionType actionType, int payload) =>
	        _items.FirstOrDefault(item => item.ActionType == actionType && item.Payload == payload);
    }
}

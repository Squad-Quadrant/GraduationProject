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

namespace Presentation.UI.Panel.ActionMenu
{
    public class ActionMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
    {
	    [SerializeField, Required, ChildGameObjectsOnly] private GameObject actionDescPanel;
        [SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI actionDesc;
        [SerializeField, Required, ChildGameObjectsOnly] private Transform itemsParent;

        [SerializeField] private string playerLockedMessage = "行动选择";
        [SerializeField] private string allyLockedMessage = "未到该单位的回合，无法操作";
        [SerializeField] private string enemyLockedMessage = "敌方单位，无法操作";

        private List<ActionMenuItem> _items;

        private bool _locked;

        public void DataInitialize(Systems.Unit.Unit unit)
        {
	        _items ??= itemsParent.GetComponentsInChildren<ActionMenuItem>(true).ToList();

            foreach (var item in _items)
            {
                item.Button.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(item.ActionType, item.Payload)));

                item.OnHoverEnter += SetActionDesc;
                item.OnHoverExit += _ => SetActionDesc(playerLockedMessage);
                item.SetActive(false);
                item.SetAudioEnabled(true);
            }
            Refresh(unit);
        }

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
		        item.Button.interactable = false;
		        item.SetAudioEnabled(false);
	        }
	        SetActionDesc(unit.faction == EUnitFaction.Player ? allyLockedMessage : enemyLockedMessage);
        }

        public void ShowActions(Systems.Unit.Unit unit)
        {
	        _locked = false;
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
	        actionDescPanel.SetActive(!string.IsNullOrEmpty(desc));
	        actionDesc.text = desc;
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

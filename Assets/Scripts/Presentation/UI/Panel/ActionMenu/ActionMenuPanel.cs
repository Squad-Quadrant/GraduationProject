using System.Collections.Generic;
using System.Linq;
using Data.Runtime;
using Data.Runtime.Events.UI;
using Data.Runtime.Events.Unit;
using Presentation.UI.Component;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Presentation.UI.Panel.ActionMenu
{
    public class ActionMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
    {
        [SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI currentActionText;
        [SerializeField, Required, ChildGameObjectsOnly] private Transform itemsParent;

        private List<ActionMenuItem> _items;

        public void DataInitialize(Systems.Unit.Unit unit)
        {
	        _items = itemsParent.GetComponentsInChildren<ActionMenuItem>(false).ToList();

            foreach (var item in _items)
            {
                item.Button.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(item.ActionType)));
                
                item.OnHoverEnter += SetDescription;
                item.OnHoverExit += ClearDescription;
                item.SetActive(false);
            }
            Refresh(unit);
        }

        protected override void OnOpen() => EventBus.Subscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);

        protected override void OnClose() => EventBus.Unsubscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);

        private void OnUnitInfoChanged(UnitInfoChangedEvent e) => Refresh(e.Unit);

        private void Refresh(Systems.Unit.Unit unit)
        {
            var availableActions = unit.GetAvailableActions();

            foreach (var action in availableActions)
            {
                var item = GetItem(action.ActionType);
                if (!item) continue;
                item.SetActive(true);
                item.Interactable = action.IsAvailable;
            }
        }

        private void SetDescription(string desc) => currentActionText.text = desc;
        private void ClearDescription(string _) => currentActionText.text = "";

        private ActionMenuItem GetItem(EActionType actionType) =>
	        _items.FirstOrDefault(item => item.ActionType == actionType);
    }
}

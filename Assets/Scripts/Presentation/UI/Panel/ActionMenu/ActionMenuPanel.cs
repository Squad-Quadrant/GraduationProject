using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Data.Runtime;
using Data.Runtime.Events.UI;
using Data.Runtime.Events.Unit;
using Presentation.UI.Component;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Presentation.UI.Panel
{
    public class ActionMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
    {
        [SerializeField, Required] private TextMeshProUGUI currentActionText;
        [SerializeField] private List<ActionMenuItem> items;

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        public void DataInitialize(Systems.Unit.Unit unit)
        {
            foreach (var item in items)
            {
                item.Button.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(item.ActionType)));
                
                item.OnHoverEnter += SetDescription;
                item.OnHoverExit += ClearDescription;
                item.Switch(false);
            }
            Refresh(unit);
            EventBus.Subscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);
        }
        
        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);
        }
        
        public void OnUnitInfoChanged(UnitInfoChangedEvent e)
        {
            Refresh(e.Unit);
        }
        
        private void Refresh(Systems.Unit.Unit unit)
        {
            var availableActions = unit.GetAvailableActions();

            foreach (var action in availableActions)
            {
                var item = GetItem(action.actionType);
                if (!item) continue;
                item.Switch(true);
                item.Interactable = action.isAvailable;
            }
        }

        private void SetDescription(string desc) => currentActionText.text = desc;
        private void ClearDescription(string _) => currentActionText.text = "";

        public ActionMenuItem GetItem(EActionType actionType)
        {
            return items.FirstOrDefault(item => item.ActionType == actionType);
        }
    }
}

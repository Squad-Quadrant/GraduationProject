using System.Collections.Generic;
using System.Linq;
using Data.Runtime;
using Data.Runtime.Events.UI;
using Presentation.UI.Component;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Presentation.UI.Panel
{
    public class ActionMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
    {
        [SerializeField, Required] private ActionMenuItem moveButton;
        [SerializeField, Required] private ActionMenuItem mainWeaponButton;
        [SerializeField, Required] private ActionMenuItem secondaryWeaponButton;
        [SerializeField, Required] private ActionMenuItem tacticalItem0Button;
        [SerializeField, Required] private ActionMenuItem tacticalItem1Button;
        [SerializeField, Required] private ActionMenuItem tacticalItem2Button;
        [SerializeField, Required] private ActionMenuItem waitButton;
        [SerializeField, Required] private TextMeshProUGUI currentActionText;
        [SerializeField] private List<ActionMenuItem> items;

        protected override void OnInitialize()
        {
            foreach (var item in items)
            {
                item.Button.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(item.ActionType)));
                item.OnHoverEnter += SetDescription;
                item.OnHoverExit += ClearDescription;
                item.Switch(false);
            }
        }

        public void DataInitialize(Systems.Unit.Unit unit)
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

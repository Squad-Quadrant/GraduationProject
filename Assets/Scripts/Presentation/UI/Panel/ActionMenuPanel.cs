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
		[SerializeField, Required] private ActionBtn moveButton;
		[SerializeField, Required] private ActionBtn mainWeaponButton;
		[SerializeField, Required] private ActionBtn secondaryWeaponButton;
		[SerializeField, Required] private ActionBtn tacticalItem0Button;
		[SerializeField, Required] private ActionBtn tacticalItem1Button;
		[SerializeField, Required] private ActionBtn tacticalItem2Button;
		[SerializeField, Required] private ActionBtn waitButton;
		[SerializeField, Required] private TextMeshProUGUI currentActionText;

        // todo: 复杂化该UI的逻辑
		protected override void OnInitialize()
		{
			moveButton?.Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Move)));
            mainWeaponButton?.Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.MainWeapon)));
            secondaryWeaponButton?.Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.SecondaryWeapon)));
            tacticalItem0Button?.Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.TacticalItem0)));
            tacticalItem1Button?.Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.TacticalItem1)));
            tacticalItem2Button?.Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.TacticalItem2)));
            waitButton?.Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Wait)));

            moveButton!.OnHoverEnter += SetDescription;
            moveButton.OnHoverExit += ClearDescription;
            mainWeaponButton!.OnHoverEnter += SetDescription;
            mainWeaponButton.OnHoverExit += ClearDescription;
            secondaryWeaponButton!.OnHoverEnter += SetDescription;
            secondaryWeaponButton.OnHoverExit += ClearDescription;
            tacticalItem0Button!.OnHoverEnter += SetDescription;
            tacticalItem0Button.OnHoverExit += ClearDescription;
            tacticalItem1Button!.OnHoverEnter += SetDescription;
            tacticalItem1Button.OnHoverExit += ClearDescription;
            tacticalItem2Button!.OnHoverEnter += SetDescription;
            tacticalItem2Button.OnHoverExit += ClearDescription;
            waitButton!.OnHoverEnter += SetDescription;
            waitButton.OnHoverExit += ClearDescription;
		}

		public void DataInitialize(Systems.Unit.Unit unit)
		{
            var availableActions = unit.GetAvailableActions();

            moveButton.Button.interactable = availableActions.Contains(EActionType.Move);
            mainWeaponButton.Button.interactable = availableActions.Contains(EActionType.MainWeapon);
            secondaryWeaponButton.Button.interactable = availableActions.Contains(EActionType.SecondaryWeapon);
            tacticalItem0Button.Button.interactable = availableActions.Contains(EActionType.TacticalItem0);
            tacticalItem1Button.Button.interactable = availableActions.Contains(EActionType.TacticalItem1);
            tacticalItem2Button.Button.interactable = availableActions.Contains(EActionType.TacticalItem2);
			waitButton.Button.interactable = availableActions.Contains(EActionType.Wait);
        }

		private void SetDescription(string desc) => currentActionText.text = desc;
		private void ClearDescription(string _) => currentActionText.text = "";
	}
}

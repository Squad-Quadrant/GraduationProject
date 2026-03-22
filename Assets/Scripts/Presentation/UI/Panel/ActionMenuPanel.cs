using Data.Runtime;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using Presentation.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
	public class ActionMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
	{
		[SerializeField] private Button moveButton;
		[SerializeField] private Button mainWeaponButton;
		[SerializeField] private Button secondaryWeaponButton;
		[SerializeField] private Button TacticalItem0Button;
		[SerializeField] private Button TacticalItem1Button;
		[SerializeField] private Button TacticalItem2Button;
		[SerializeField] private Button waitButton;

        // todo: 复杂化该UI的逻辑
		protected override void OnInitialize()
		{
			moveButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Move)));
            mainWeaponButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.MainWeapon)));
            secondaryWeaponButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.SecondaryWeapon)));
            TacticalItem0Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.TacticalItem0)));
            TacticalItem1Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.TacticalItem1)));
            TacticalItem2Button?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.TacticalItem2)));
            waitButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Wait)));
		}

		public void DataInitialize(Systems.Unit.Unit unit)
		{
            var availableActions = unit.GetAvailableActions();

            moveButton.interactable = availableActions.Contains(EActionType.Move);
            mainWeaponButton.interactable = availableActions.Contains(EActionType.MainWeapon);
            secondaryWeaponButton.interactable = availableActions.Contains(EActionType.SecondaryWeapon);
            TacticalItem0Button.interactable = availableActions.Contains(EActionType.TacticalItem0);
            TacticalItem1Button.interactable = availableActions.Contains(EActionType.TacticalItem1);
            TacticalItem2Button.interactable = availableActions.Contains(EActionType.TacticalItem2);
			waitButton.interactable = availableActions.Contains(EActionType.Wait);
        }
	}
}

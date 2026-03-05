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
		[SerializeField] private Button attackButton;
		[SerializeField] private Button waitButton;

		protected override void OnInitialize()
		{
			moveButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Move)));
            attackButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Attack)));
            waitButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Wait)));
		}

		public void DataInitialize(Systems.Unit.Unit unit)
		{
            var availableActions = unit.GetAvailableActions();

            moveButton.interactable = availableActions.Contains(EActionType.Move);
			attackButton.interactable = availableActions.Contains(EActionType.Attack);
			waitButton.interactable = availableActions.Contains(EActionType.Wait);
        }
	}
}

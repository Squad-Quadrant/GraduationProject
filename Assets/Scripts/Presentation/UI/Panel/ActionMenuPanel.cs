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

		protected override void OnInitialize()
		{
			moveButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Move)));
		}

		public void DataInitialize(Systems.Unit.Unit selectedUnit)
		{
			// todo: use data to populate the menu with available actions for the selected unit
		}
	}
}

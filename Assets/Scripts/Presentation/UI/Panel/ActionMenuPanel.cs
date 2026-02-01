using Data.Runtime;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using Presentation.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
	public class ActionMenuPanel : UIPanel, IInitializable<UnitSelectedEvent>
	{
		[SerializeField] private Button moveButton;

		protected override void OnInitialize()
		{
			moveButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Move)));
		}

		public void Initialize(UnitSelectedEvent data)
		{
			// Set up the action menu based on the selected unit
		}
	}
}

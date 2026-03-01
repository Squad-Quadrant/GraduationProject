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
		[SerializeField] private Button attackButton;
		[SerializeField] private Button waitButton;
		[SerializeField] private Button endTurnButton;
        [SerializeField] private Text currentUnit;

		protected override void OnInitialize()
		{
			moveButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Move)));
            attackButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Attack)));
            waitButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Wait)));
            endTurnButton?.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.EndTurn)));
		}

		public void Initialize(UnitSelectedEvent data)
		{
			// Set up the action menu based on the selected unit
		}

        public void SetUnit(string id)
        {
            currentUnit.text = id;
        }
	}
}

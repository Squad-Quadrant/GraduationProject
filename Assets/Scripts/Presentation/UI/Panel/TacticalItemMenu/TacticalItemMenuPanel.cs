using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Systems.Unit.Equipment;
using TMPro;
using UnityEngine;

namespace Presentation.UI.Panel.TacticalItemMenu
{
	public class TacticalItemMenuPanel : UIPanel, IInitializable<EquipmentContainer>
	{
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI titleText;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI descText;
		[SerializeField, Required, ChildGameObjectsOnly] private TacticalItemDetailDisplayPanel tacticalItemDetailDisplayPanel;

		[Title("Defaults")]
		[SerializeField] private string defaultTitleText;
		[SerializeField] private string defaultDescText;

		public void DataInitialize(EquipmentContainer container)
		{
			if (container.IsNullOrEmpty())
			{
				titleText.text = defaultTitleText;
				descText.text = defaultDescText;
				tacticalItemDetailDisplayPanel.Default();
				tacticalItemDetailDisplayPanel.gameObject.SetActive(false);
				return;
			}

			titleText.text = container.Config.nName;
			descText.text = container.Config.description;
			tacticalItemDetailDisplayPanel.gameObject.SetActive(true);
			tacticalItemDetailDisplayPanel.Show(container);
		}
	}
}

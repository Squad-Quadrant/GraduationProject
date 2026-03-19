using Presentation.UI.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
	public class UnitInfoPanel : UIPanel, IInitializable<Systems.Unit.Unit>
	{
		[TitleGroup("References")]
		[SerializeField, Required] private Image portraitImage;
		[SerializeField, Required] private TextMeshProUGUI nameText;
		[SerializeField, Required] private Image hpImage;
		[SerializeField, Required] private RectTransform actionPointsParent;
		[SerializeField, Required] private GameObject actionPointsPrefab;

		public void DataInitialize(Systems.Unit.Unit unit)
		{
			if (unit == null) return;
			Refresh(unit);
		}

		public void Refresh(Systems.Unit.Unit unit)
		{
			portraitImage.sprite = unit.icon;
			nameText.text = unit.name;

			var currentHp = unit.currentHp;
			var maxHp = unit.maxHp;
			hpImage.fillAmount = maxHp > 0 ? (float)currentHp / maxHp : 0f;

			foreach (Transform child in actionPointsParent)
				Destroy(child.gameObject);
			for (int i = 0; i < unit.currentAp; i++)
				Instantiate(actionPointsPrefab, actionPointsParent);
		}
	}
}

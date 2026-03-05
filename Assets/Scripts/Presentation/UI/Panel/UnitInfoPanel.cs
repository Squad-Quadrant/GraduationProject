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
		[SerializeField, Required] private Image iconImage;
		[SerializeField, Required] private TextMeshProUGUI nameText;
		[SerializeField, Required] private TextMeshProUGUI factionText;

		[SerializeField, Required] private Slider hpSlider;
		[SerializeField, Required] private TextMeshProUGUI hpText;

		[SerializeField, Required] private TextMeshProUGUI actionPointsText;
		[SerializeField, Required] private TextMeshProUGUI speedText;

		public void DataInitialize(Systems.Unit.Unit unit)
		{
			if (unit == null) return;
			Refresh(unit);
		}

		public void Refresh(Systems.Unit.Unit unit)
		{
			iconImage.sprite = unit.icon;
			nameText.text = unit.name;
			factionText.text = FormatFaction(unit.faction);

			var currentHp = unit.currentHp;
			var maxHp = unit.maxHp;
			hpSlider.value = maxHp > 0 ? (float)currentHp / maxHp : 0f;
			hpText.text = $"{currentHp} / {maxHp}";

			actionPointsText.text = $"AP: {unit.currentAp}/{unit.maxAp}";
			speedText.text = $"Speed: {unit.speed}";
		}

		private static string FormatFaction(Systems.Unit.EUnitFaction? faction)
		{
			return faction switch
			{
				Systems.Unit.EUnitFaction.Player  => "Player",
				Systems.Unit.EUnitFaction.Enemy   => "Enemy",
				Systems.Unit.EUnitFaction.Neutral => "Neutral",
				_ => "Unknown"
			};
		}
	}
}

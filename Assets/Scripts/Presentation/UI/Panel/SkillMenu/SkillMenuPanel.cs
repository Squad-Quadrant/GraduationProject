using Data.Runtime.Events.Unit;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.UI.Panel.SkillMenu
{
	public class SkillMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
	{
		[Title("Slot")]
		[SerializeField, Required] private SkillMenuSlot slot;

		private Systems.Unit.Unit _unit;

		public void DataInitialize(Systems.Unit.Unit unit)
		{
			_unit = unit;
			Refresh(unit);
		}

		protected override void OnOpen() => EventBus.Subscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);

		protected override void OnClose() => EventBus.Unsubscribe<UnitInfoChangedEvent>(OnUnitInfoChanged);

		private void OnUnitInfoChanged(UnitInfoChangedEvent e)
		{
			if (_unit == null || e.Unit.id != _unit.id) return;
			Refresh(e.Unit);
		}

		private void Refresh(Systems.Unit.Unit unit)
		{
			if (unit.Skill == null)
			{
				if (slot) slot.gameObject.SetActive(false);
				return;
			}

			slot.gameObject.SetActive(true);
			slot.Bind(unit.Skill, interactable: unit.Skill.CanUse);
		}
	}
}

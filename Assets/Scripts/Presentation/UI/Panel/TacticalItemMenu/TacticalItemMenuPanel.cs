using System.Collections.Generic;
using Data.Runtime.Events.Unit;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Systems.Unit.Equipment;
using UnityEngine;

namespace Presentation.UI.Panel.TacticalItemMenu
{
	public class TacticalItemMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
	{
		[Title("Slots")]
		[SerializeField, Required] private List<TacticalItemSlot> slots;

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
			bool canUse = unit.HasAp && unit.CanUseEquipment; // todo: 没有加入剩余数量判断

			for (int i = 0; i < slots.Count; i++)
			{
				var slot = slots[i];
				if (!slot) continue;

				var equipmentContainer = unit.GetTacticalItem(i);
				if (equipmentContainer.IsNullOrEmpty())
				{
					slot.gameObject.SetActive(false);
					continue;
				}

				slot.gameObject.SetActive(true);
				slot.Bind(i, equipmentContainer, interactable: canUse);
			}
		}
	}
}

using Data.Runtime;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using Presentation.Interaction;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment;
using Systems.Unit.Equipment.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.TacticalItemMenu
{
	public class TacticalItemMenuPanel : UIPanel, IInitializable<Systems.Unit.Unit>
	{
		[SerializeField, Required, ChildGameObjectsOnly] private Transform slotRoot;
		[SerializeField, Required, AssetsOnly] private TacticalItemSlot slotPrefab;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI titleText;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI descText;
		[SerializeField] private string defaultTitleText;
		[SerializeField] private string defaultDescText;
		[SerializeField, Required, ChildGameObjectsOnly] private TacticalItemDetailDisplayPanel tacticalItemDetailDisplayPanel;
		[SerializeField, Required, ChildGameObjectsOnly] private GameObject confirmPanel;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI confirmText;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI confirmDescText;
		[SerializeField, Required, ChildGameObjectsOnly] private Button confirmButton;
		[SerializeField, Required, ChildGameObjectsOnly] private Button backButton;

		private TacticalItemSlot _currentSelectedSlot;
		private InteractionController _interactionController;

		public void Init(InteractionController interactionController)
		{
			_interactionController = interactionController;
		}

		public void DataInitialize(Systems.Unit.Unit unit)
		{
			SetupSlots(unit);

			tacticalItemDetailDisplayPanel.Default();
			tacticalItemDetailDisplayPanel.gameObject.SetActive(false);
			confirmPanel.SetActive(false);
			confirmButton.interactable = false;
		}

		protected override void OnOpen()
		{
			EventBus.Subscribe<TargetingEvent>(OnTargeting);
			confirmButton.onClick.AddListener(() => EventBus.Publish(new TargetConfirmEvent()));
			backButton.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Back)));
		}

		protected override void OnClose()
		{
			EventBus.Unsubscribe<TargetingEvent>(OnTargeting);
			confirmButton.onClick.RemoveAllListeners();
			backButton.onClick.RemoveAllListeners();
		}

		private void SetupSlots(Systems.Unit.Unit unit)
		{
			bool unitCanAct = unit.HasAp;
			_currentSelectedSlot = null;

			for (int i = 2; i >= 0; i--)
			{
				var equipmentContainer = unit.GetTacticalItem(i);
				if (equipmentContainer.IsNullOrEmpty() || equipmentContainer.Logic is not TacticalItemLogic tacticalItemLogic) continue;

				var slot = Instantiate(slotPrefab, slotRoot);
				slot.transform.SetAsFirstSibling();
				bool slotCanUse = unitCanAct && equipmentContainer.Logic is TacticalItemLogic { CanUse: true };
				slot.Setup(i, equipmentContainer, slotCanUse, tacticalItemLogic.RemainingUses);

				slot.Button.onClick.RemoveAllListeners();
				slot.Button.onClick.AddListener(() =>
				{
					if (_currentSelectedSlot)
						_currentSelectedSlot.SetInteractable(true);
					_currentSelectedSlot = slot;
					slot.SetInteractable(false);
					SetConfirmAndDescText(equipmentContainer.Config.nName, equipmentContainer.Config.battleDescription);
					confirmPanel.SetActive(true);
					RefreshTacticalItemDetailDisplayPanel(null);
					EventBus.Publish(new TacticalItemSelectedEvent(slot.SlotIndex));
				});

				slot.PointerEnter = () =>
				{
					if (!slot.Button.interactable) return;
					SetTitleAndDescText(equipmentContainer.Config.nName, equipmentContainer.Config.description);
					RefreshTacticalItemDetailDisplayPanel(slot.Container);
					EventBus.Publish(new RangeDisplayEvent(ERangeType.Interact, (tacticalItemLogic as ITargeted)?.GetValidCells(_interactionController.Context)));
				};

				slot.PointerExit = () =>
				{
					SetTitleAndDescText(defaultTitleText, defaultDescText);
					RefreshTacticalItemDetailDisplayPanel(null);

					if (!_currentSelectedSlot)
						EventBus.Publish(RangeDisplayEvent.Clear(ERangeType.Interact));
				};
			}
		}

		private void OnTargeting(TargetingEvent e)
		{
			if (!e.TargetCell.HasValue)
			{
				if (_currentSelectedSlot)
					_currentSelectedSlot.SetInteractable(true);
				_currentSelectedSlot = null;
				SetTitleAndDescText(defaultTitleText, defaultDescText);
				SetConfirmAndDescText("", "");
				confirmPanel.SetActive(false);
				RefreshTacticalItemDetailDisplayPanel(null);
			}

			confirmButton.interactable = e.TargetCell.HasValue;
		}

		private void RefreshTacticalItemDetailDisplayPanel(EquipmentContainer container)
		{
			if (container != null) // 临时展示
			{
				tacticalItemDetailDisplayPanel.gameObject.SetActive(true);
				tacticalItemDetailDisplayPanel.Show(container);
				return;
			}

			if (!_currentSelectedSlot) // 平时展示当前选中
			{
				tacticalItemDetailDisplayPanel.Default();
				tacticalItemDetailDisplayPanel.gameObject.SetActive(false);
				return;
			}

			tacticalItemDetailDisplayPanel.gameObject.SetActive(true);
			tacticalItemDetailDisplayPanel.Show(_currentSelectedSlot.Container);
		}

		private void SetTitleAndDescText(string title, string desc)
		{
			titleText.text = title;
			descText.text = desc;
		}

		private void SetConfirmAndDescText(string title, string desc)
		{
			confirmText.text = title;
			confirmDescText.text = desc;
		}
	}
}

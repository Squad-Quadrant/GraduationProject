using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.UI;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Systems.Damage;
using Systems.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.AttachPreview
{
    public class AttackPreviewPanel : UIPanel, IInitializable<Systems.Unit.Unit>
    {
	    [SerializeField, ChildGameObjectsOnly, Required] private TextMeshProUGUI modeTmp;
	    [SerializeField, ChildGameObjectsOnly, Required] private TextMeshProUGUI modeDescTmp;
	    [SerializeField, ChildGameObjectsOnly, Required] private TextMeshProUGUI confirmModeTmp;
	    [SerializeField, ChildGameObjectsOnly, Required] private TextMeshProUGUI confirmModeDescTmp;
	    [SerializeField, ChildGameObjectsOnly, Required] private AttackMenuItem normalAttackItem;
	    [SerializeField, ChildGameObjectsOnly, Required] private AttackMenuItem preciseAttackItem;
	    [SerializeField, ChildGameObjectsOnly, Required] private AttackContextDisplayPanel attackContextDisplayPanel;
	    [SerializeField, ChildGameObjectsOnly, Required] private Button confirmButton;
	    [SerializeField, ChildGameObjectsOnly, Required] private Button backButton;

	    [ShowInInspector, ReadOnly] private Vector2Int? _targetCell;

	    private AttackMenuItem _currentSelectedItem;

	    private Systems.Unit.Unit _currentUnit;

	    private IDamageService _damageService;
	    private IUnitService _unitService;

	    public void Init(IDamageService damageService, IUnitService unitService)
	    {
		    _damageService = damageService;
		    _unitService = unitService;
	    }

        protected override void OnOpen()
        {
	        EventBus.Subscribe<DisplayAttackContextEvent>(OnDisplayAttackContext);
	        EventBus.Subscribe<TargetingEvent>(OnTargeting);
        }

        protected override void OnClose()
        {
	        EventBus.Unsubscribe<DisplayAttackContextEvent>(OnDisplayAttackContext);
	        EventBus.Unsubscribe<TargetingEvent>(OnTargeting);
        }

        public void DataInitialize(Systems.Unit.Unit unit)
        {
	        SetupButtons(unit);

	        attackContextDisplayPanel.Default();

	        confirmButton.interactable = false;
        }

        private void SetupButtons(Systems.Unit.Unit unit)
        {
	        _currentUnit = unit;
	        _currentSelectedItem = normalAttackItem;

	        bool canPreciseShoot = unit.CurrentWeaponLogic.CanPreciseShoot();

	        normalAttackItem.PointerEnter = () =>
	        {
		        if (!normalAttackItem.Button.interactable) return;
		        SetModeText(normalAttackItem.mode, normalAttackItem.desc);
	        };
	        normalAttackItem.PointerExit = () => SetModeText(_currentSelectedItem.mode, _currentSelectedItem.desc);

	        normalAttackItem.Button.onClick.RemoveAllListeners();
	        normalAttackItem.Button.onClick.AddListener(() =>
	        {
		        normalAttackItem.SetInteractable(false);
		        if (canPreciseShoot) preciseAttackItem.SetInteractable(true);
		        _currentSelectedItem = normalAttackItem;
		        unit.CurrentWeaponLogic.IsOnPreciseShoot = false;
		        SetConfirmModeText(normalAttackItem.mode, $"使用{unit.CurrentWeaponLogic.DisplayName}向目标进行{unit.CurrentWeaponLogic.ShootSpeed()}发直接射击");
		        RefreshAttackContextDisplay(unit);
	        });

	        preciseAttackItem.Button.interactable = canPreciseShoot;
	        preciseAttackItem.PointerEnter = () =>
	        {
		        if (!preciseAttackItem.Button.interactable) return;
		        SetModeText(preciseAttackItem.mode, preciseAttackItem.desc);
	        };
	        preciseAttackItem.PointerExit = () => SetModeText(_currentSelectedItem.mode, _currentSelectedItem.desc);

	        preciseAttackItem.Button.onClick.RemoveAllListeners();
	        preciseAttackItem.Button.onClick.AddListener(() =>
	        {
		        normalAttackItem.SetInteractable(true);
		        preciseAttackItem.SetInteractable(false);
		        _currentSelectedItem = preciseAttackItem;
		        unit.CurrentWeaponLogic.IsOnPreciseShoot = true;
		        SetConfirmModeText(preciseAttackItem.mode, $"使用{unit.CurrentWeaponLogic.DisplayName}向目标进行{unit.CurrentWeaponLogic.PreciseShootSpeed()}发高精度射击");
		        RefreshAttackContextDisplay(unit);
	        });

	        backButton.onClick.RemoveAllListeners();
	        backButton.onClick.AddListener(() => EventBus.Publish(new ActionSelectedEvent(EActionType.Back)));

	        normalAttackItem.Button.onClick.Invoke();
        }

        private void OnTargeting(TargetingEvent e)
        {
	        _targetCell = e.TargetCell;
	        confirmButton.interactable = _targetCell.HasValue;
        }

        private void OnDisplayAttackContext(DisplayAttackContextEvent e)
        {
	        if (_targetCell.HasValue) return;

	        if (e.Context == null)
	        {
		        attackContextDisplayPanel.Default();
		        return;
	        }

	        RefreshAttackContextDisplay(e.Context, _currentUnit);
        }

        private void RefreshAttackContextDisplay(DamageExecutingContext context, Systems.Unit.Unit attacker) =>
	        attackContextDisplayPanel.Show(context, attacker);

        private void RefreshAttackContextDisplay(Systems.Unit.Unit attacker)
        {
	        if (!_targetCell.HasValue) return;

	        var target = _unitService.GetUnitAtPosition(_targetCell.Value);
	        if (target == null)
	        {
		        this.LogError($"{_targetCell} 存在，但是UnitService中无法找到");
		        return;
	        }

	        var attackContext = _damageService.GetSimulatedDamage(new BulletDamageTriggeringInfo(attacker, target, EActionType.Attack));
	        RefreshAttackContextDisplay(attackContext, attacker);
        }

        private void SetModeText(string mode, string desc)
        {
	        modeTmp.text = mode;
	        modeDescTmp.text = desc;
        }

        private void SetConfirmModeText(string mode, string desc)
        {
	        confirmModeTmp.text = mode;
	        confirmModeDescTmp.text = desc;
        }
    }
}

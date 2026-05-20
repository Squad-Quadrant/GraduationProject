using System;
using System.Linq;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.View;
using DG.Tweening;
using Presentation.Audio;
using Systems.Damage;
using Systems.Map;
using Systems.Unit;

namespace Data.Runtime.Commands
{
	public class UnitAttackCommand : AsyncCommand
	{
		private readonly string _unitId;
        private readonly string _targetUnitId;
        private readonly int _apCost;
        private readonly int _damage;
        private readonly EActionType  _actionType;

		private readonly IUnitService _unitService;
		private readonly IMapService _mapService;
		private readonly IEventBus _eventBus;
		private readonly AudioService _audioService;

		private Unit _attacker;

		private bool _fireResolved;
		private bool _attackAnimationDone;
		private bool _beHitAnimationDone;
		private bool _destroyAnimationDone;

		public bool WaitForAnimation { get; set; } = true;

		private Action<UnitAttackFiredEvent> _onUnitAttackFired;
		private Action<PresentationCompleteEvent> _onPresentationComplete;

		public override string Name => $"Attack({_unitId} → {_targetUnitId})";
		public override bool CanUndo => true;
        
		public UnitAttackCommand(
			string unitId,
            string targetUnitId,
            int apCost,
            EActionType  actionType,
			IUnitService unitService,
			IMapService mapService,
			IEventBus eventBus,
			AudioService audioService)
		{
			_unitId = unitId;
            _targetUnitId = targetUnitId;
            _apCost = apCost;
            _actionType =  actionType;
            
			_unitService = unitService;
			_mapService = mapService;
			_eventBus = eventBus;
			_audioService = audioService;
		}

		protected override void OnExecuteAsync()
		{
			this.Log($"Executing: {Name}");

			if (!_unitService.TryGetUnit(_unitId, out _attacker))
			{
				this.LogError($"Unit '{_unitId}' not found!");
				CompleteExecution();
				return;
			}

			if (!_unitService.TryGetUnit(_targetUnitId, out _))
			{
				this.LogError($"Target unit '{_targetUnitId}' not found!");
				CompleteExecution();
				return;
			}

            _attacker.CanAttack.Value = false;
            _attacker.CurrentAp -= _apCost;

            _eventBus.Publish(new UnitAttackStartedEvent(_unitId, _targetUnitId));

            if (WaitForAnimation)
			{
				_onUnitAttackFired = OnUnitAttackFired;
				_onPresentationComplete = OnPresentationComplete;
				_eventBus.Subscribe(_onUnitAttackFired);
				_eventBus.Subscribe(_onPresentationComplete);
			}
			else
            {
	            ResolveFire();
	            _eventBus.Publish(new UnitAttackEndedEvent(_unitId, _targetUnitId));
                CompleteExecution();
            }
		}

		protected override void OnUndoAsync()
        {
            // todo:
			CompleteUndo();
		}

		private void OnUnitAttackFired(UnitAttackFiredEvent e)
		{
			if (e.AttackerId != _unitId) return;
			ResolveFire();
		}

		private void ResolveFire()
		{
			if (_fireResolved) return;
			_fireResolved = true;

			this.Log($"Fire resolved for {_unitId} → {_targetUnitId}");

			if (!_unitService.TryGetUnit(_targetUnitId, out var targetUnit))
			{
				this.LogError($"Target unit '{_targetUnitId}' not found at fire time!");
				return;
			}

			var weaponLogic = _attacker.CurrentWeaponLogic;
			var weaponConfig = weaponLogic.WeaponConfig;

			_audioService.PlaySfx(
				weaponLogic.CurrentAmmo() <= 0 ? weaponConfig.emptyClip : weaponConfig.fireClip,
				5);

			var info = new BulletDamageTriggeringInfo(_attacker, targetUnit, _actionType);
			_eventBus.Publish(new DealDamageEvent(info));
		}

		private void OnPresentationComplete(PresentationCompleteEvent e)
        {
	        if (!e.Matches(EPresentationCategory.Animation, PresentationType.Animation.Attack, _unitId))
		        return;

	        this.Log($"Animation complete for {_unitId}");

	        DOVirtual.DelayedCall(0.5f, () =>
	        {
		        Cleanup();
		        _eventBus.Publish(new UnitAttackEndedEvent(_unitId, _targetUnitId));
		        CompleteExecution();
	        }).SetUpdate(true);
        }

		private void Cleanup()
		{
			if (_onUnitAttackFired != null)
			{
				_eventBus.Unsubscribe(_onUnitAttackFired);
				_onUnitAttackFired = null;
			}

			if (_onPresentationComplete != null)
			{
				_eventBus.Unsubscribe(_onPresentationComplete);
				_onPresentationComplete = null;
			}
		}
	}
}

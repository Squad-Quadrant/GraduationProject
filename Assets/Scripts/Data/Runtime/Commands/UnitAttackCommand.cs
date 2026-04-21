using System;
using System.Linq;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.View;
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

        private bool _attackAnimationDone = false;
        private bool _beHitAnimationDone = false;

		public bool WaitForAnimation { get; set; } = true;

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
			IEventBus eventBus)
		{
			_unitId = unitId;
            _targetUnitId = targetUnitId;
            _apCost = apCost;
            _actionType =  actionType;
            
			_unitService = unitService;
			_mapService = mapService;
			_eventBus = eventBus;
		}

		protected override void OnExecuteAsync()
		{
			this.Log($"Executing: {Name}");

			if (!_unitService.TryGetUnit(_unitId, out var unit))
			{
				this.LogError($"Unit '{_unitId}' not found!");
				CompleteExecution();
				return;
			}
            
            if (!_unitService.TryGetUnit(_targetUnitId, out var targetUnit))
            {
                this.LogError($"Target unit '{_targetUnitId}' not found!");
                CompleteExecution();
                return;
            }

            // _eventBus.Publish(new UnitBeHitEvent(unit));
            var info = new BulletDamageTriggeringInfo(unit, targetUnit, _actionType);
            _eventBus.Publish(new DealDamageEvent(info));
            
			unit.CanAttack.Value = false;
            unit.CurrentAp -= _apCost;
            
			if (WaitForAnimation)
			{
				_onPresentationComplete = OnPresentationComplete;
				_eventBus.Subscribe(_onPresentationComplete);
			}
			else
            {
                var units = _unitService.GetAllAliveUnits().ToList();
                CompleteExecution();
            }
		}

		protected override void OnUndoAsync()
        {
            // todo:
			CompleteUndo();
		}

		private void OnPresentationComplete(PresentationCompleteEvent e)
        {
            if (!e.Matches(EPresentationCategory.Animation, PresentationType.Animation.Attack, _unitId))
                _attackAnimationDone = true;
            
            if (!e.Matches(EPresentationCategory.Animation, PresentationType.Animation.BeHit, _targetUnitId))
                _beHitAnimationDone = true;
            
            if (!_attackAnimationDone || !_beHitAnimationDone) return;

			this.Log($"Animation complete for {_unitId}");

			Cleanup();
			CompleteExecution();
		}

		private void Cleanup()
		{
			if (_onPresentationComplete == null) return;
			_eventBus.Unsubscribe(_onPresentationComplete);
			_onPresentationComplete = null;
		}
	}
}

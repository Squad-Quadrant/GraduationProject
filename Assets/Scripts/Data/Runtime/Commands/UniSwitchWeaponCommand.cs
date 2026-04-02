using System;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.View;
using Systems.Unit;

namespace Data.Runtime.Commands
{
	public class UnitSwitchWeaponCommand : AsyncCommand
	{
		private readonly Unit _unit;
		private readonly int _apCost;

		private readonly IEventBus _eventBus;

		public bool WaitForAnimation { get; set; } = false;

		private Action<PresentationCompleteEvent> _onPresentationComplete;

        public override string Name => $"{_unit.name} 更换武器";
		public override bool CanUndo => false;


		public UnitSwitchWeaponCommand(
            Unit unit,
			int apCost,
			IEventBus eventBus)
		{
			_unit = unit;
			_apCost = apCost;
			_eventBus = eventBus;
		}

		protected override void OnExecuteAsync()
		{
			this.Log($"Executing: {Name}");

			if (_unit == null)
			{
				this.LogError($"Unit is null!");
				CompleteExecution();
				return;
			}

            _unit.CurrentAp -= _apCost;
            _unit.SwitchWeapon();
            _eventBus.Publish(new UnitReloadedEvent(_unit));
            
			if (WaitForAnimation)
			{
				_onPresentationComplete = OnPresentationComplete;
				_eventBus.Subscribe(_onPresentationComplete);
			}
			else
            {
                CompleteExecution();
            }
		}
        

		private void OnPresentationComplete(PresentationCompleteEvent e)
		{
			if (!e.Matches(EPresentationCategory.Animation, PresentationType.Animation.SwitchWeapon, _unit.id))
				return;

			this.Log($"Animation complete for {_unit.id}");

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

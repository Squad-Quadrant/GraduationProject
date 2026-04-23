using System;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.View;
using Systems.Unit;

namespace Data.Runtime.Commands
{
	public class UnitReloadCommand : AsyncCommand
	{
		private readonly Unit _unit;
		private readonly int _apCost;

		private readonly IEventBus _eventBus;

		public bool WaitForAnimation { get; set; } = true;

		private Action<PresentationCompleteEvent> _onPresentationComplete;

		public override string Name => $"{_unit.name} 装弹";
		public override bool CanUndo => false;


		public UnitReloadCommand(
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
            _unit.CurrentWeaponLogic.CurrentAmmo(1000);

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
			if (!e.Matches(EPresentationCategory.Animation, PresentationType.Animation.Reload, _unit.id))
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

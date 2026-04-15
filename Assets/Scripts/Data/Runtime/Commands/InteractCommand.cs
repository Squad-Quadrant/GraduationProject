using System;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.View;
using Systems.Map.SceneActor;
using Systems.Unit;

namespace Data.Runtime.Commands
{
	public class InteractCommand : AsyncCommand
	{
		private readonly Unit _unit;
		private readonly InteractableSceneActor _actor;
		private readonly IEventBus _eventBus;

		private Action<PresentationCompleteEvent> _onPresentationComplete;

		public override string Name => $"与 {_actor} 交互";
		public override bool CanUndo => false;

		public InteractCommand(Unit unit, InteractableSceneActor actor, IEventBus eventBus)
		{
			_unit = unit;
			_actor = actor;
			_eventBus = eventBus;
		}

		protected override void OnExecuteAsync()
		{
			this.Log($"Executing: {Name}");

			if (_actor == null)
			{
				this.LogError($"Actor is null!");
				CompleteExecution();
				return;
			}

			_unit.CurrentAp -= 1;

			_onPresentationComplete = OnPresentationComplete;
			_eventBus.Subscribe(_onPresentationComplete);

			_actor.Interact();
		}

		private void OnPresentationComplete(PresentationCompleteEvent e)
		{
			if (!e.Matches(EPresentationCategory.Interact))
				return;

			this.Log($"Interact Complete: {Name}");

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

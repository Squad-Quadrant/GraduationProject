using System;
using Core.Events;
using Core.FSM;
using Core.Log;
using Presentation.UI.Core;
using Presentation.UI.Panel.AttackPreview;
using Systems.Interaction;

namespace Presentation.UI.Presenter
{
	public class AttackPreviewPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;

		private AttackPreviewPanel _attackPreviewPanel;

		public AttackPreviewPresenter(UIManager uiManager, IEventBus eventBus)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

			_eventBus.Subscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);

			if (_attackPreviewPanel)
			{
				_uiManager.Close<AttackPreviewPanel>();
				_attackPreviewPanel = null;
			}
		}

		private void OnStateChanged(StateChangedEvent<InteractionContext> e)
		{
			var current = e.CurrentState?.Name;
			var previous = e.PreviousState?.Name;

			if (previous == InteractionStates.AttackPreview && _attackPreviewPanel)
			{
				_uiManager.Close<AttackPreviewPanel>();
				_attackPreviewPanel = null;
			}


			if (current == InteractionStates.AttackPreview)
			{
				_attackPreviewPanel = _uiManager.Open<AttackPreviewPanel, Systems.Unit.Unit>(e.Context.selectedUnit);
			}
		}
	}
}

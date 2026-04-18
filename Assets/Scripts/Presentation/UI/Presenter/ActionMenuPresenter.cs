using System;
using Core.Events;
using Core.FSM;
using Core.Log;
using Presentation.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel;
using Presentation.UI.Panel.ActionMenu;
using Systems.Damage;
using Systems.Interaction;
using Systems.Unit;

namespace Presentation.UI.Presenter
{
	public class ActionMenuPresenter : IDisposable
	{
		private readonly UIManager _uiManager;
        private readonly IDamageService _damageService;
        private readonly IUnitService _unitService;
		private readonly IEventBus _eventBus;
		private ActionMenuPanel _actionMenuPanel;
        private AttackPreviewPanel  _attackPreviewPanel;
        private readonly InteractionContext _interactionContext;

		public ActionMenuPresenter(UIManager uiManager, IEventBus eventBus, IDamageService damageService, IUnitService unitService, 
            InteractionContext interactionContext)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _damageService = damageService ?? throw new ArgumentNullException(nameof(damageService));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _interactionContext = interactionContext ?? throw new ArgumentNullException(nameof(interactionContext));
			_eventBus.Subscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);
		}

        private void OnStateChanged(StateChangedEvent<InteractionContext> e)
        {
            var current = e.CurrentState?.Name;
            var previous = e.PreviousState?.Name;

            if (previous == InteractionStates.UnitSelected && _actionMenuPanel)
            {
                _uiManager.Close<ActionMenuPanel>();
                _actionMenuPanel = null;
            }

            if (previous == InteractionStates.AttackPreview && _attackPreviewPanel)
            {
                _uiManager.Close<AttackPreviewPanel>();
                _attackPreviewPanel = null;
            }
            
            
            if (current == InteractionStates.UnitSelected)
            {
                _actionMenuPanel = _uiManager.Open<ActionMenuPanel, Systems.Unit.Unit>(e.Context.selectedUnit);
            }

            if (current == InteractionStates.AttackPreview)
            {
                _attackPreviewPanel = _uiManager.Open<AttackPreviewPanel, Systems.Unit.Unit>(e.Context.selectedUnit);
                _attackPreviewPanel.Init(_damageService, _unitService, _interactionContext);
            }
        }
    }
}

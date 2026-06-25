using System;
using Core.Events;
using Core.FSM;
using Data.Runtime.Events.Input;
using Presentation.Audio;
using Presentation.Bootstrap;
using Presentation.CameraControl;
using Presentation.UI.Core;
using Presentation.UI.Panel;
using Systems.Interaction;
using Systems.Time;

namespace Presentation.UI.Presenter
{
	public class BattleSettingPresenter : IDisposable
	{
		private UIManager UIManager { get; }
		private IEventBus EventBus { get; }
		private AudioService AudioService { get; }
		private GameFlowController GameFlowController { get; }
		private CameraController CameraController { get; }
		private ITimeService TimeService { get; }

		private BattleSettingPanel _battleSettingPanel;
		private bool _openable;

		public BattleSettingPresenter(UIManager uiManager,
			IEventBus eventBus,
			AudioService audioService,
			GameFlowController gameFlowController,
			CameraController cameraController,
			ITimeService timeService)
		{
			UIManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			AudioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
			GameFlowController = gameFlowController ?? throw new ArgumentNullException(nameof(gameFlowController));
			CameraController = cameraController ?? throw new ArgumentNullException(nameof(cameraController));
			TimeService = timeService ?? throw new ArgumentNullException(nameof(timeService));

			EventBus.Subscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);
			EventBus.Subscribe<EscInputEvent>(OnEscInput);
		}

		public void Dispose()
		{
			EventBus.Unsubscribe<StateChangedEvent<InteractionContext>>(OnStateChanged);
			EventBus.Unsubscribe<EscInputEvent>(OnEscInput);

			if (_battleSettingPanel)
			{
				UIManager.Close<BattleSettingPanel>();
				_battleSettingPanel = null;
			}
		}

		private void OnStateChanged(StateChangedEvent<InteractionContext> e)
		{
			var current = e.CurrentState?.Name;
			var previous = e.PreviousState?.Name;

			_openable = current == InteractionStates.UnitSelected;

			if (previous != InteractionStates.UnitSelected || !_battleSettingPanel) return;
			UIManager.Close<BattleSettingPanel>();
			_battleSettingPanel = null;
		}

		private void OnEscInput(EscInputEvent e)
		{
			if (_openable && !_battleSettingPanel)
			{
				_battleSettingPanel = UIManager.Open<BattleSettingPanel, SettingPanelData>(new SettingPanelData
				{
					AudioService = AudioService,
					CameraController = CameraController,
					TimeService = TimeService,
					OnReturnToMenu = () =>
					{
						UIManager.Close<BattleSettingPanel>();
						_battleSettingPanel = null;
						GameFlowController.ReturnToMainMenu();
					},
					OnBack = () =>
					{
						UIManager.Close<BattleSettingPanel>();
						_battleSettingPanel = null;
					}
				});
			}
			else
			{
				UIManager.Close<BattleSettingPanel>();
				_battleSettingPanel = null;
			}
		}
	}
}

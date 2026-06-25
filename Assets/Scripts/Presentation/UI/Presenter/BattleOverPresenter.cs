using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.BattleFlow;
using DG.Tweening;
using Presentation.Bootstrap;
using Presentation.Dialogue;
using Presentation.Dialogue.Config;
using Presentation.UI.Core;
using Presentation.UI.Panel.BattleFlow;

namespace Presentation.UI.Presenter
{
	public class BattleOverPresenter : IDisposable
	{
		private const float BannerDelay = 2.5f;

		private readonly UIManager _uiManager;
		private readonly IEventBus _eventBus;
		private readonly GameFlowController _gameFlowController;
		private readonly DialogueController _dialogueController;
		private readonly DialogueConfig _victoryDialogue;
		private readonly DialogueConfig _defeatDialogue;

		private Tween _delayTween;

		private BattleOverPanel _panel;

		public BattleOverPresenter(
			UIManager uiManager,
			IEventBus eventBus,
			GameFlowController gameFlowController,
			DialogueController dialogueController,
			DialogueConfig victoryDialogue,
			DialogueConfig defeatDialogue)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_gameFlowController = gameFlowController ?? throw new ArgumentNullException(nameof(gameFlowController));
			_dialogueController = dialogueController ?? throw new ArgumentNullException(nameof(dialogueController));
			_victoryDialogue = victoryDialogue;
			_defeatDialogue = defeatDialogue;

			_eventBus.Subscribe<BattleOverEvent>(OnBattleOver);
			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<BattleOverEvent>(OnBattleOver);
			_delayTween?.Kill();

			if (_panel)
			{
				_uiManager.Close<BattleOverPanel>();
				_panel = null;
			}
			this.Log("Disposed");
		}

		private void OnBattleOver(BattleOverEvent e)
		{
			this.Log($"BattleOverEvent received: {(e.IsVictory ? "Victory" : "Defeat")}, scheduling banner in {BannerDelay}s");

			_delayTween?.Kill();
			_delayTween = DOVirtual.DelayedCall(
				BannerDelay,
				() =>
				{
					_delayTween = null;
					var dialogue = e.IsVictory ? _victoryDialogue : _defeatDialogue;
					if (dialogue)
						_dialogueController.Play(dialogue, () => ShowPanel(e.IsVictory));
					else
						ShowPanel(e.IsVictory);
				},
				ignoreTimeScale: true);
		}

		private void ShowPanel(bool isVictory)
		{
			_panel = _uiManager.Open<BattleOverPanel, BattleOverPanelData>(new BattleOverPanelData
			{
				IsVictory = isVictory,
				OnRestart = _gameFlowController.RestartCurrentLevel,
				OnReturnToMenu = _gameFlowController.ReturnToMainMenu,
			});
		}
	}
}

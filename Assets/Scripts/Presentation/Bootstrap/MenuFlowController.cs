using Core.Log;
using Data;
using Data.Config;
using Presentation.UI.Core;
using Presentation.UI.Panel.Menu;
using Presentation.UI.Panel.Menu.Loadout;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Bootstrap
{
	public class MenuFlowController : MonoBehaviour
	{
		private UIManager _uiManager;
		private DataManager _dataManager;
		private GameFlowController _gameFlowController;

		private UIPanel _openedPanel;
		private LevelConfig _selectedLevel;

		private void Awake()
		{
			_uiManager = RootContainer.Instance.Resolve<UIManager>();
			_dataManager = RootContainer.Instance.Resolve<DataManager>();
			_gameFlowController = RootContainer.Instance.Resolve<GameFlowController>();
		}

		private void Start() => ShowMainMenu();

		private void ShowMainMenu()
		{
			if (_openedPanel) _uiManager.Close(_openedPanel);
			_openedPanel = _uiManager.Open<MainMenuPanel, MainMenuPanelData>(new MainMenuPanelData
			{
				OnStart = ShowLevelSelect,
				OnQuit = QuitGame,
			});
		}

		private void ShowLevelSelect()
		{
			if (_openedPanel) _uiManager.Close(_openedPanel);
			_openedPanel = _uiManager.Open<LevelSelectPanel, LevelSelectPanelData>(new LevelSelectPanelData
			{
				OnLevelSelected = ShowLoadout,
				OnBack = ShowMainMenu,
			});
		}

		private void ShowLoadout(LevelConfig level)
		{
			if (!level)
			{
				this.LogError("Cannot show loadout: level is null");
				return;
			}

			_selectedLevel = level;

			if (_openedPanel) _uiManager.Close(_openedPanel);
			_openedPanel = _uiManager.Open<LoadoutPanel, LoadoutPanelData>(new LoadoutPanelData
			{
				Level = level,
				DataManager = _dataManager,
				OnStartBattle = StartBattle,
				OnBack = ShowLevelSelect,
			});
		}

		private void StartBattle()
		{
			if (!_selectedLevel)
			{
				this.LogError("Cannot start battle: no level selected");
				return;
			}

			if (_openedPanel) _uiManager.Close(_openedPanel);
			_gameFlowController.StartBattle(_selectedLevel);
		}

		private void QuitGame()
		{
			this.Log("Quitting game");
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}

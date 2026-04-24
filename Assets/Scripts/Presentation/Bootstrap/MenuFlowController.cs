using System;
using Core.Log;
using Data;
using Data.Config;
using Presentation.UI.Core;
using Presentation.UI.Panel.Menu;
using Presentation.UI.Panel.Menu.Loadout;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Presentation.Bootstrap
{
	public class MenuFlowController : MonoBehaviour
	{
		[Title("Scene")]
		[SerializeField, Tooltip("主菜单场景名，必须与 Build Settings 中的名字一致")]
		private string mainMenuSceneName = "0_MainMenu";

		[SerializeField, Tooltip("关卡场景名，必须与 Build Settings 中的名字一致")]
		private string battleSceneName = "1_Battle";

		private UIManager _uiManager;
		private DataManager _dataManager;

		private void Awake()
		{
			_uiManager = RootContainer.Instance.Resolve<UIManager>();
			_dataManager = RootContainer.Instance.Resolve<DataManager>();
		}

		private void Start() => ShowMainMenu();

		private void ShowMainMenu()
		{
			_uiManager.CloseAll();
			_uiManager.Open<MainMenuPanel, MainMenuPanelData>(new MainMenuPanelData
			{
				OnStart = ShowLevelSelect,
				OnQuit = QuitGame,
			});
		}

		private void ShowLevelSelect()
		{
			_uiManager.CloseAll();
			_uiManager.Open<LevelSelectPanel, LevelSelectPanelData>(new LevelSelectPanelData
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

			_dataManager.SelectedLevel = level;

			_uiManager.CloseAll();
			_uiManager.Open<LoadoutPanel, LoadoutPanelData>(new LoadoutPanelData
			{
				Level = level,
				DataManager = _dataManager,
				OnStartBattle = StartBattle,
				OnBack = ShowLevelSelect,
			});
		}

		private void StartBattle()
		{
			if (!_dataManager.SelectedLevel)
			{
				this.LogError("Cannot start battle: SelectedLevel is null");
				return;
			}

			_uiManager.CloseAll();
			this.Log($"Loading Battle scene for level '{_dataManager.SelectedLevel.levelId}'");
			SceneManager.LoadScene(battleSceneName);
		}

		public void ReturnToMainMenu()
		{
			this.Log("Returning to main menu");
			_dataManager.SelectedLevel = null;
			SceneManager.LoadScene(mainMenuSceneName);
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

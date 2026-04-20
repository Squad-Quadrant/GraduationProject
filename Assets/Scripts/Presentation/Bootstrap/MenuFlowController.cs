using System;
using Core.Log;
using Data;
using Data.Config;
using Presentation.UI.Core;
using Presentation.UI.Panel.Menu;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Presentation.Bootstrap
{
	public class MenuFlowController : MonoBehaviour
	{
		[Title("Scene")]
		[SerializeField, Tooltip("关卡场景名，必须与 Build Settings 中的名字一致")]
		private string battleSceneName = "1_Battle";

		private UIManager _uiManager;
		private DataManager _dataManager;

		public void Initialize(UIManager uiManager, DataManager dataManager)
		{
			_uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
			_dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
			this.Log("Initialized");
		}

		// 入口：主菜单
		public void ShowMainMenu()
		{
			_uiManager.CloseAll();
			_uiManager.Open<MainMenuPanel, MainMenuPanelData>(new MainMenuPanelData
			{
				OnStart = ShowLevelSelect,
				OnQuit = QuitGame,
			});
		}

		// 入口：选关
		private void ShowLevelSelect()
		{
			_uiManager.CloseAll();
			_uiManager.Open<LevelSelectPanel, LevelSelectPanelData>(new LevelSelectPanelData
			{
				OnLevelSelected = StartBattle,
				OnBack = ShowMainMenu,
			});
		}

		// 入口：开始游戏
		private void StartBattle(LevelConfig level)
		{
			if (!level)
			{
				this.LogError("Cannot start battle: level is null");
				return;
			}

			_dataManager.SelectedLevel = level;
			_uiManager.CloseAll();

			this.Log($"Loading Battle scene for level '{level.levelId}'");
			SceneManager.LoadScene(battleSceneName);
		}

		public void ReturnToMainMenu()
		{
			this.Log("Returning to main menu");
			_dataManager.SelectedLevel = null;

			SceneManager.sceneLoaded += ShowMainMenu;
			SceneManager.LoadScene(BootstrapEnsure.BootstrapperSceneName);
		}

		private void ShowMainMenu(Scene scene, LoadSceneMode mode)
		{
			if (scene.name != BootstrapEnsure.BootstrapperSceneName) return;

			SceneManager.sceneLoaded -= ShowMainMenu;
			ShowMainMenu();
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

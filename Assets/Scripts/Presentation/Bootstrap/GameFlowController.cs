using Core.Log;
using Data;
using Data.Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Bootstrap
{
	public class GameFlowController : MonoBehaviour
	{
		[Title("Scenes")]
		[SerializeField, Tooltip("主菜单场景名，必须与 Build Settings 中一致")]
		private string mainMenuSceneName = "0_MainMenu";

		[SerializeField, Tooltip("战斗场景名，必须与 Build Settings 中一致")]
		private string battleSceneName = "1_Battle";

		private SceneTransitioner _sceneTransitioner;
		private DataManager _dataManager;
		private bool _initialized;

		public void Initialize()
		{
			if (_initialized) return;
			_initialized = true;

			_sceneTransitioner = RootContainer.Instance.Resolve<SceneTransitioner>();
			_dataManager = RootContainer.Instance.Resolve<DataManager>();

			this.Log("Initialized");
		}

		public void StartBattle(LevelConfig level)
		{
			if (!_initialized)
			{
				this.LogError("Not initialized");
				return;
			}
			if (!level)
			{
				this.LogError("Cannot start battle: level is null");
				return;
			}

			_dataManager.SelectedLevel = level;
			this.Log($"StartBattle: '{level.levelId}'");
			_sceneTransitioner.LoadScene(battleSceneName, waitForLevelLoaded: true);
		}

		public void RestartCurrentLevel()
		{
			if (!_initialized)
			{
				this.LogError("Not initialized");
				return;
			}
			if (!_dataManager.SelectedLevel)
			{
				this.LogError("Cannot restart: SelectedLevel is null");
				return;
			}

			this.Log($"RestartCurrentLevel: '{_dataManager.SelectedLevel.levelId}'");
			_sceneTransitioner.LoadScene(battleSceneName, waitForLevelLoaded: true);
		}

		public void ReturnToMainMenu()
		{
			if (!_initialized)
			{
				this.LogError("Not initialized");
				return;
			}

			_dataManager.SelectedLevel = null;
			this.Log("ReturnToMainMenu");
			_sceneTransitioner.LoadScene(mainMenuSceneName, waitForLevelLoaded: false);
		}
	}
}

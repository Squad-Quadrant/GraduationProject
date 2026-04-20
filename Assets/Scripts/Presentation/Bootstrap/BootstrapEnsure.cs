using UnityEngine;
using UnityEngine.SceneManagement;

namespace Presentation.Bootstrap
{
	public static class BootstrapEnsure
	{
		public const string BootstrapperSceneName = "0_Bootstrapper";

		private static string _devStartScene;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Ensure()
		{
			var activeScene = SceneManager.GetActiveScene();
			if (activeScene.name == BootstrapperSceneName) return;

			_devStartScene = activeScene.name;
			Debug.Log($"[BootstrapEnsure] Dev scene '{_devStartScene}' detected; rerouting via Bootstrapper.");
			SceneManager.LoadScene(BootstrapperSceneName);
		}

		// 非空返回值表示"从该场景启动"，Bootstrapper 应 LoadScene 回那个场景而非进主菜单
		public static string ConsumeDevStartScene()
		{
			var s = _devStartScene;
			_devStartScene = null;
			return s;
		}
	}
}

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Presentation.Bootstrap
{
	public static class BootstrapEnsure
	{
		private const string BootstrapperResourcePath = "Bootstrapper";

		private static bool _instantiated;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset() => _instantiated = false;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void AutoBootstrap()
		{
			if (_instantiated) return;
			_instantiated = true;

			var prefab = Resources.Load<GameObject>(BootstrapperResourcePath);
			if (!prefab)
			{
				Debug.LogError(
					$"[BootstrapEnsure] Prefab not found: Resources/{BootstrapperResourcePath}.prefab. " +
					"Global services will NOT be initialized.");
				return;
			}
			Object.Instantiate(prefab);

			Debug.Log("[BootstrapEnsure] Bootstrapper instantiated from prefab.");
		}
	}
}

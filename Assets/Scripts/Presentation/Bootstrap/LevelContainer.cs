using UnityEngine;

namespace Presentation.Bootstrap
{
	/// <summary>
	/// Level-specific service container
	/// </summary>
	public class LevelContainer : MonoBehaviour
	{
		public static LevelContainer Instance { get; private set; }

		public ServiceContainer Services { get; private set; }

		/// <summary>
		/// Called during level loading
		/// </summary>
		public void Initialize()
		{
			if (Instance && Instance != this)
			{
				Debug.LogWarning("[LevelContainer] Multiple LevelContainer instances detected. Destroying duplicate.");
				Destroy(gameObject);
				return;
			}

			Instance = this;
			Services = new ServiceContainer(RootContainer.Instance.Services);
			Debug.Log("[LevelContainer] Initialized");
		}

		public T Resolve<T>() => Services.Resolve<T>();

		public T TryResolve<T>() where T : class => Services.TryResolve<T>();

		public void Clear()
		{
			if (Instance != this) return;
			Debug.Log("[LevelContainer] Cleaning up...");
			Services?.Clear();
			Instance = null;
			Debug.Log("[LevelContainer] Cleanup complete.");
		}
	}
}

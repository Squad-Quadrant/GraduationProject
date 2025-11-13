using UnityEngine;

namespace Presentation.Bootstrap
{
	/// <summary>
	/// Level-specific service container
	/// </summary>
	public class LevelContainer : MonoBehaviour
	{
		private static LevelContainer _instance;
		public static LevelContainer Instance
		{
			get
			{
				if (!_instance)
					Debug.LogError("No active LevelContainer. Please ensure a LevelContainer exists in the scene.");
				return _instance;
			}
		}

		public ServiceContainer Services { get; private set; }

		/// <summary>
		/// Called during level loading
		/// </summary>
		public void Initialize()
		{
			if (_instance && _instance != this)
			{
				Debug.LogWarning("Multiple LevelContainer instances detected. Destroying duplicate.");
				Destroy(gameObject);
				return;
			}

			_instance = this;
			Services = new ServiceContainer(RootContainer.Instance.Services);
			Debug.Log("[LevelContainer] Initialized");
		}

		public T Resolve<T>() => Services.Resolve<T>();

		public T TryResolve<T>() where T : class => Services.TryResolve<T>();

		public void Cleanup()
		{
			Debug.Log("[LevelContainer] Cleaning up...");
			Services?.Clear();
			if (_instance == this)
				_instance = null;
			Debug.Log("[LevelContainer] Cleanup complete.");
		}

		private void OnDisable()
		{
			if (_instance == this) Cleanup();
		}

		private void OnDestroy() => Cleanup();

		private void OnApplicationQuit() => Services?.Clear(); // Ensure cleanup on application quit

		public void RegisterServices()
		{
			Debug.Log("[LevelContainer] Registering services...");

			// todo: Register level-specific services here

			Debug.Log($"[LevelContainer] Service registration complete.");
		}
	}
}

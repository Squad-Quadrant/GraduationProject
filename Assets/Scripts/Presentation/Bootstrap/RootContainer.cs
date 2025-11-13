using Core.Events;
using Core.Log;
using Presentation.Services;
using UnityEngine;

namespace Presentation.Bootstrap
{
	/// <summary>
	/// Global service container
	/// </summary>
	public class RootContainer : MonoBehaviour
	{
		private static RootContainer _instance;
		public static RootContainer Instance
		{
			get
			{
				if (!_instance)
					Debug.LogError("RootContainer instance is not initialized. Please ensure a RootContainer exists in the scene.");
				return _instance;
			}
		}

		public ServiceContainer Services { get; private set; }

		/// <summary>
		/// Called by Bootstrapper during initialization
		/// </summary>
		public void Initialize()
		{
			if (_instance && _instance != this)
			{
				Debug.LogWarning("Multiple RootContainer instances detected. Destroying duplicate.");
				Destroy(gameObject);
				return;
			}

			_instance = this;
			DontDestroyOnLoad(gameObject);
			Services = new ServiceContainer();
			Debug.Log("[RootContainer] Initialized");
		}

		public T Resolve<T>() => Services.Resolve<T>();

		public T TryResolve<T>() where T : class => Services.TryResolve<T>();

		private void OnDestroy()
		{
			if (_instance != this) return;
			Debug.Log("[RootContainer] Cleaning up...");
			Services?.Clear();
			_instance = null;
			Debug.Log("[LevelContainer] Cleanup complete.");
		}

		private void OnApplicationQuit() => Services?.Clear(); // Ensure cleanup on application quit

		public void RegisterServices()
		{
			Debug.Log("[RootContainer] Registering services...");

			Services.Register<ILog, UnityLog>();
			Services.Register<IEventBus, EventBus>();

			Debug.Log($"[RootContainer] Service registration complete.");
		}
	}
}

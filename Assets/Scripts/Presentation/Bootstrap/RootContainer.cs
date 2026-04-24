using Core.Commands;
using Core.Events;
using UnityEngine;

namespace Presentation.Bootstrap
{
	/// <summary>
	/// Global service container
	/// </summary>
	public class RootContainer : MonoBehaviour
	{
		public static RootContainer Instance { get; private set; }

		public ServiceContainer Services { get; private set; }

		/// <summary>
		/// Called by Bootstrapper during initialization
		/// </summary>
		public void Initialize()
		{
			if (Instance && Instance != this)
			{
				Debug.LogWarning("Multiple RootContainer instances detected. Destroying duplicate.");
				Destroy(gameObject);
				return;
			}

			Instance = this;
			Services = new ServiceContainer();
			Debug.Log("[RootContainer] Initialized");
		}

		public T Resolve<T>() => Services.Resolve<T>();

		public T TryResolve<T>() where T : class => Services.TryResolve<T>();

		private void OnDestroy()
		{
			if (Instance != this) return;
			Debug.Log("[RootContainer] Cleaning up...");
			Services?.Clear();
			Instance = null;
			Debug.Log("[RootContainer] Cleanup complete.");
		}

		public void RegisterServices()
		{
			Debug.Log("[RootContainer] Registering services...");

			Services.Register<IEventBus, EventBus>();
			Services.Register<ICommandQueue, CommandQueue>();

			Debug.Log($"[RootContainer] Service registration complete.");
		}
	}
}

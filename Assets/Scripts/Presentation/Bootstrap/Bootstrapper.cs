using Core.Events;
using Core.Log;
using Data.Config;
using Presentation.Data;
using Presentation.Logger;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Bootstrap
{
	public class Bootstrapper : MonoBehaviour
	{
		[TitleGroup("Settings")]
		[SerializeField] private bool autoInitialize = true;
		[SerializeField] private bool enableLogs = true;

		[TitleGroup("Configuration")]
		[SerializeField, Required, InlineEditor]
		private LogSettings logSettings;

		[TitleGroup("Configuration")]
		[SerializeField, Required]
		private UIManager uiManager;
        
        [SerializeField, Required]
        private DataManager dataManager;

		[TitleGroup("Prefabs")]
		[SerializeField]
		private RootContainer rootContainerPrefab;

		private static bool _initialized;
		private RootContainer _rootContainerInstance;

		private void Awake()
		{
			if (autoInitialize)
				Initialize();
		}

		private void Initialize()
		{
			if (_initialized) return; // Prevent double initialization
			_initialized = true;

			Log("====================================");
			Log("[Bootstrapper] Initialization started...");

			InitializeRootContainer();

			RegisterGlobalServices();

			OnBootstrapComplete();

			Log("[Bootstrapper] Initialization complete.");
			Log("====================================");
		}

		private void InitializeRootContainer()
		{
			Log("[Bootstrapper] Initializing RootContainer...");
			_rootContainerInstance = FindObjectOfType<RootContainer>();
			if (!_rootContainerInstance)
			{
				if (rootContainerPrefab)
				{
					_rootContainerInstance = Instantiate(rootContainerPrefab);
					_rootContainerInstance.name = "RootContainer";
					Log("[Bootstrapper] RootContainer instantiated from prefab.");
				}
				else
				{
					var rootObj = new GameObject("RootContainer");
					_rootContainerInstance = rootObj.AddComponent<RootContainer>();
					Log("[Bootstrapper] RootContainer created as new GameObject.");
				}
			}
			_rootContainerInstance.Initialize();
			Log("[Bootstrapper] RootContainer initialized.");
		}

		private void RegisterGlobalServices()
		{
			// initialize logger
			_rootContainerInstance.Services.Register<ILoggerFactory>(_ => new UnityLoggerFactory(logSettings));
			LogExtensions.Initialize(_rootContainerInstance.Services.Resolve<ILoggerFactory>());

			_rootContainerInstance.RegisterServices();

			_rootContainerInstance.Services.RegisterInstance(uiManager);
			uiManager.Initialize(_rootContainerInstance.Resolve<IEventBus>());
            
            _rootContainerInstance.Services.RegisterInstance(dataManager);
            
			Log("[Bootstrapper] Global services registered.");
		}

		private void OnBootstrapComplete()
		{
			// Notify other systems that bootstrap is complete
		}

		#region Debug

		private void Log(string message)
		{
			if (enableLogs) Debug.Log(message);
		}

		#endregion
	}
}

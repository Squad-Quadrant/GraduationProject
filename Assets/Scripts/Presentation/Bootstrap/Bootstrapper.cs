using Core.Events;
using Core.Log;
using Data;
using Data.Config;
using Presentation.Audio;
using Presentation.Logger;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Bootstrap
{
	public class Bootstrapper : MonoBehaviour
	{
		[TitleGroup("Settings")]
		[SerializeField] private bool enableLogs = true;

		[TitleGroup("Configuration")]
		[SerializeField, Required, InlineEditor]
		private LogSettings logSettings;

		[TitleGroup("Configuration")]
		[SerializeField, Required]
		private UIManager uiManager;

		[TitleGroup("Configuration")]
        [SerializeField, Required]
        private DataManager dataManager;

		[TitleGroup("Configuration")]
        [SerializeField, Required]
        private AudioService audioService;

		[TitleGroup("Prefabs")]
		[SerializeField, ReadOnly]
		private RootContainer rootContainerPrefab;

		private bool _initialized;
		private RootContainer _rootContainerInstance;

		private void Awake() => Initialize();

		private void Initialize()
		{
			if (_initialized) return; // Prevent double initialization
			_initialized = true;
			DontDestroyOnLoad(gameObject);

			Log("====================================");
			Log("[Bootstrapper] Initialization started...");

			InitializeRootContainer();

			RegisterGlobalServices();

			Log("[Bootstrapper] Initialization complete.");
			Log("====================================");
		}

		private void InitializeRootContainer()
		{
			Log("[Bootstrapper] Initializing RootContainer...");

			var rootObj = new GameObject("RootContainer");
			rootObj.transform.SetParent(transform);
			_rootContainerInstance = rootObj.AddComponent<RootContainer>();
			Log("[Bootstrapper] RootContainer created as new GameObject.");

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

            _rootContainerInstance.Services.RegisterInstance(audioService);
            audioService.Initialize();
            
			Log("[Bootstrapper] Global services registered.");
		}

		#region Debug

		private void Log(string message)
		{
			if (enableLogs) Debug.Log(message);
		}

		#endregion
	}
}

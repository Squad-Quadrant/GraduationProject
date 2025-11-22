using Data.Config;
using Presentation.Map;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map;
using UnityEngine;

namespace Presentation.Bootstrap
{
	public class Bootstrapper : MonoBehaviour
	{
		[Title("Settings")]
		[SerializeField] private bool autoInitialize = true;

		[Title("Prefabs")]
		[SerializeField] private RootContainer rootContainerPrefab;

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

			Debug.Log("====================================");
			Debug.Log("[Bootstrapper] Initialization started...");

			InitializeRootContainer();

			RegisterGlobalServices();

			OnBootstrapComplete();

			Debug.Log("[Bootstrapper] Initialization complete.");
			Debug.Log("====================================");
		}

		private void InitializeRootContainer()
		{
			Debug.Log("[Bootstrapper] Initializing RootContainer...");
			_rootContainerInstance = FindObjectOfType<RootContainer>();
			if (!_rootContainerInstance)
			{
				if (rootContainerPrefab)
				{
					_rootContainerInstance = Instantiate(rootContainerPrefab);
					_rootContainerInstance.name = "RootContainer";
					Debug.Log("[Bootstrapper] RootContainer instantiated from prefab.");
				}
				else
				{
					var rootObj = new GameObject("RootContainer");
					_rootContainerInstance = rootObj.AddComponent<RootContainer>();
					Debug.Log("[Bootstrapper] RootContainer created as new GameObject.");
				}
			}
			_rootContainerInstance.Initialize();
			Debug.Log("[Bootstrapper] RootContainer initialized.");
		}

		private void RegisterGlobalServices() => _rootContainerInstance.RegisterServices();

		private void OnBootstrapComplete()
		{
			// Notify other systems that bootstrap is complete
			Debug.Log("[Bootstrapper] Bootstrap process completed successfully.");
		}

		/// <summary>
		/// Creates and returns a new LevelContainer instance.
		/// </summary>
		/// <returns></returns>
		public static LevelContainer CreateLevelContainer()
		{
			var levelObj = new GameObject("LevelContainer");
			var levelContainer = levelObj.AddComponent<LevelContainer>();
			levelContainer.Initialize();
			levelContainer.RegisterServices();

			// MapLoading Example

			var grid = FindObjectOfType<Grid>();
			var coordinateConverter = new CoordinateConverter(grid);
			levelContainer.Services.RegisterInstance<ICoordinateConverter>(coordinateConverter);

			// temp Error
			// levelContainer.Services.Register<IMapService>(container =>
			// {
			// 	var mapConfig = Resources.Load<MapConfig>("");
			// 	var mapData = new MapData();
			// 	var service = new MapService(mapData);
			// 	service.LoadFromConfig(mapConfig);  // 加载配置
			// 	return service;
			// });

			Debug.Log("[Bootstrapper] LevelContainer created and initialized.");
			return levelContainer;
		}
	}
}

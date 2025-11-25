using System;
using System.Collections;
using Core.Events;
using Data.Config;
using Data.Runtime.Events;
using Presentation.Map;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map;
using Systems.Turn;
using Systems.Unit;
using UnityEngine;

namespace Presentation.Bootstrap
{
	/// <summary>
	/// 关卡加载引导脚本，目前只完成了Map、Unit、Turn服务的初始化和注册
	/// todo: 还需要手动触发Map的渲染、Unit的实例化、Turn的开始等逻辑
	/// </summary>
	public class LevelLoader : MonoBehaviour
	{
		[Title("References")]
		[SerializeField] private MapView mapView;
		[SerializeField] private Grid grid;

		[Title("Configuration")]
		[SerializeField] private LevelConfig levelConfig;
		[SerializeField] private bool autoLoadLevel = false;

		private LevelContainer _levelContainer;
		private IMapService _mapService;
		private IUnitService _unitService;
		private ITurnService _turnService;
		private IEventBus _eventBus;

		private void Start()
		{
			if (autoLoadLevel && levelConfig)
				LoadLevel(levelConfig);
		}

		private void OnDestroy() => UnloadLevel();

		public void LoadLevel(LevelConfig config)
		{
			if (!config)
			{
				Debug.LogError("[LevelLoader] Cannot load null level config!");
				return;
			}

			levelConfig = config;
			StartCoroutine(LoadLevelCoroutine());
		}

		public void UnloadLevel()
		{
			Debug.Log("[LevelLoader] Unloading level...");

			// Clear all services
			_turnService?.Clear();
			_unitService?.Clear();

			// Destroy level container
			if (_levelContainer != null)
			{
				_levelContainer.Cleanup();
				Destroy(_levelContainer.gameObject);
				_levelContainer = null;
			}

			// Clear references
			_mapService = null;
			_unitService = null;
			_turnService = null;
			_eventBus = null;

			Debug.Log("[LevelLoader] Level unloaded successfully.");
		}

		private IEnumerator LoadLevelCoroutine()
		{
			Debug.Log("====================================");
			Debug.Log("[LevelLoader] Loading level...");

			yield return RegisterServices();
			yield return InitializeMap();
			yield return CreateUnits();

			Debug.Log($"[LevelLoader] Level '{levelConfig.levelName}' loaded successfully!");
			Debug.Log("====================================");

			_eventBus.Publish(new LevelLoadedEvent(levelConfig.levelId, levelConfig.levelName));
		}

		private IEnumerator CreateLevelContainer()
		{
			Debug.Log("[LevelLoader] Creating LevelContainer...");

			// Create level container GameObject
			var containerObj = new GameObject("LevelContainer");
			_levelContainer = containerObj.AddComponent<LevelContainer>();
			_levelContainer.Initialize();

			Debug.Log("[LevelLoader] ✓ LevelContainer created.");
			yield return null;
		}

		private IEnumerator RegisterServices()
		{
			Debug.Log("[LevelLoader] Registering services...");

			// Get EventBus from RootContainer (global service)
			_eventBus = RootContainer.Instance.Resolve<IEventBus>();

			// Register coordinate converter (uses scene's Grid component)
			if (!grid)
				grid = FindObjectOfType<Grid>();

			if (!grid)
			{
				Debug.LogError("[LevelLoader] No Grid found in scene! Please add a Grid component.");
				yield break;
			}

			var coordinateConverter = new CoordinateConverter(grid);
			_levelContainer.Services.RegisterInstance<ICoordinateConverter>(coordinateConverter);

			// Register MapService
			_levelContainer.Services.Register<IMapService, MapService>();

			// Register UnitService
			_levelContainer.Services.Register<IUnitService, UnitService>();

			// Register TurnService
			_levelContainer.Services.Register<ITurnService, TurnService>();

			// Resolve services
			_mapService = _levelContainer.Resolve<IMapService>();
			_unitService = _levelContainer.Resolve<IUnitService>();
			_turnService = _levelContainer.Resolve<ITurnService>();

			Debug.Log("[LevelLoader] ✓ Services registered and resolved.");
			yield return null;
		}

		private IEnumerator InitializeMap()
		{
			Debug.Log("[LevelLoader] Initializing map...");

			if (!levelConfig.mapConfig)
			{
				Debug.LogError("[LevelLoader] No MapConfig assigned to LevelConfig!");
				yield break;
			}

			// Load map from config
			_mapService.LoadFromConfig(levelConfig.mapConfig);

			Debug.Log($"[LevelLoader] ✓ Map initialized: {levelConfig.mapConfig.mapName} {levelConfig.mapConfig.size.x}x{levelConfig.mapConfig.size.y})");
			yield return null;
		}

		private IEnumerator CreateUnits()
		{
			Debug.Log("[LevelLoader] Spawning units...");

			if (levelConfig.unitPlacements.Count == 0)
			{
				Debug.LogWarning("[LevelLoader] No units to spawn!");
				yield break;
			}

			foreach (var placement in levelConfig.unitPlacements)
			{
				try
				{
					// Validate placement
					if (!placement.unitConfig)
					{
						Debug.LogError($"[LevelLoader] Unit '{placement.unitId}' has no config!");
						continue;
					}

					// Check if position is valid
					if (!_mapService.Data.IsInBounds(placement.startPosition))
					{
						Debug.LogError($"[LevelLoader] Unit '{placement.unitId}' spawn position {placement.startPosition} is out of bounds!");
						continue;
					}

					// Create unit through UnitService
					var unit = _unitService.CreateUnit(
						placement.unitId,
						placement.unitConfig,
						placement.startPosition
					);

					// Occupy cell on map
					_mapService.OccupyCell(placement.startPosition, placement.unitId);

					Debug.Log($"[LevelLoader] ✓ Spawned {unit.Name} at {placement.startPosition}");
				}
				catch (Exception ex)
				{
					Debug.LogError($"[LevelLoader] Failed to spawn unit '{placement.unitId}': {ex.Message}");
				}

				yield return null;
			}

			Debug.Log($"[LevelLoader] ✓ Spawned {levelConfig.unitPlacements.Count} units.");
		}
	}
}

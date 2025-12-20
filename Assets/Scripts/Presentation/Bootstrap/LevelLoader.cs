using System;
using System.Collections;
using Core.Events;
using Core.Log;
using Data.Config;
using Data.Runtime.Events;
using Presentation.Input;
using Presentation.Map;
using Sirenix.OdinInspector;
using Systems.GamePlay;
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
		[SerializeField] private InputService inputService;

		[Title("Configuration")]
		[SerializeField] private LevelConfig levelConfig;
		[SerializeField] private bool autoLoadLevel = false;

		private LevelContainer _levelContainer;
		private IEventBus _eventBus;
		private IMapService _mapService;
		private IUnitService _unitService;
		private ITurnService _turnService;
		private IGameServer _gameServer;

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
				this.LogError("Cannot load null level config!");
				return;
			}

			levelConfig = config;
			StartCoroutine(LoadLevelCoroutine());
		}

		public void UnloadLevel()
		{
			this.Log("Unloading level...");

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

			this.Log("Level unloaded successfully.");
		}

		private IEnumerator LoadLevelCoroutine()
		{
			this.Log("====================================", false);
			this.Log("Loading level...");

			yield return CreateLevelContainer();
			yield return RegisterServices();
			yield return InitializeMap();
			yield return CreateUnits();
			yield return InitializeInputService();

			this.Log($"Level '{levelConfig.levelName}' loaded successfully!");
			this.Log("====================================", false);

			_gameServer.StartGame();
			_eventBus.Publish(new LevelLoadedEvent(levelConfig.levelId, levelConfig.levelName));
		}

		private IEnumerator CreateLevelContainer()
		{
			this.Log("Creating LevelContainer...");

			// Create level container GameObject
			var containerObj = new GameObject("LevelContainer");
			_levelContainer = containerObj.AddComponent<LevelContainer>();
			_levelContainer.Initialize();

			this.Log("✓ LevelContainer created.");
			yield return null;
		}

		private IEnumerator RegisterServices()
		{
			this.Log("Registering services...");

			// Get EventBus from RootContainer (global service)
			_eventBus = RootContainer.Instance.Resolve<IEventBus>();

			// Register coordinate converter (uses scene's Grid component)
			if (!grid)
				grid = FindObjectOfType<Grid>();

			if (!grid)
			{
				this.LogError("No Grid found in scene! Please add a Grid component.");
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

			_levelContainer.Services.Register<IGameServer, GameServer>();

			// Resolve services
			_mapService = _levelContainer.Resolve<IMapService>();
			_unitService = _levelContainer.Resolve<IUnitService>();
			_turnService = _levelContainer.Resolve<ITurnService>();
			_gameServer = _levelContainer.Resolve<IGameServer>();


			this.Log("✓ Services registered and resolved.");
			yield return null;
		}

		private IEnumerator InitializeMap()
		{
			this.Log("Initializing map...");

			if (!levelConfig.mapConfig)
			{
				this.LogError("No MapConfig assigned to LevelConfig!");
				yield break;
			}

			// Load map from config
			_mapService.LoadFromConfig(levelConfig.mapConfig);
            mapView.RenderTerrain(_mapService.Data);
            
			this.Log($"✓ Map initialized: {levelConfig.mapConfig.MapName} {levelConfig.mapConfig.Size.x}x{levelConfig.mapConfig.Size.y})");
			yield return null;
		}

		private IEnumerator CreateUnits()
		{
			this.Log("Spawning units...");

			if (levelConfig.unitPlacements.Count == 0)
			{
				this.LogWarning("No units to spawn!");
				yield break;
			}

			foreach (var placement in levelConfig.unitPlacements)
			{
				try
				{
					// Validate placement
					if (!placement.unitConfig)
					{
						this.LogError($"Unit '{placement.unitId}' has no config!");
						continue;
					}

					// Check if position is valid
					if (!_mapService.Data.IsInBounds(placement.startPosition))
					{
						this.LogError($"Unit '{placement.unitId}' spawn position {placement.startPosition} is out of bounds!");
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

					this.Log($"✓ Spawned {unit.name} at {placement.startPosition}");
				}
				catch (Exception ex)
				{
					this.LogError($"Failed to spawn unit '{placement.unitId}': {ex.Message}");
				}

				yield return null;
			}

			this.Log($"✓ Spawned {levelConfig.unitPlacements.Count} units.");
		}

		private IEnumerator InitializeInputService()
		{
			this.Log("Initializing InputService...");

			if (!inputService)
			{
				inputService = FindObjectOfType<InputService>();
				if (!inputService)
				{
					this.LogError("No InputService found in scene!");
					yield break;
				}
			}

			inputService.Initialize(
				_eventBus,
				_levelContainer.Resolve<ICoordinateConverter>(),
				_mapService
			);
		}
	}
}

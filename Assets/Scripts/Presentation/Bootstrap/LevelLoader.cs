using System;
using System.Collections;
using System.Collections.Generic;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Config;
using Data.Runtime.Events;
using Data.Runtime.Events.Map;
using Presentation.Input;
using Presentation.Interaction;
using Presentation.Map;
using Presentation.UI.Core;
using Presentation.UI.Presenter;
using Sirenix.OdinInspector;
using Systems.GamePlay;
using Systems.Interfaces;
using Systems.Map;
using Systems.PathFinding;
using Systems.PathFinding.TraversalRule;
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
		[SerializeField] private InteractionController interactionController;

		[Title("Configuration")]
		[SerializeField] private LevelConfig levelConfig;
		[SerializeField] private bool autoLoadLevel = false;

		private LevelContainer _levelContainer;
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
				this.LogError("Cannot load null level config!");
				return;
			}

			levelConfig = config;
			StartCoroutine(LoadLevelCoroutine());
		}

		public void UnloadLevel()
		{
			this.Log("====================================", false);
			this.Log("Unloading level...");

			// Clear all services
			_levelContainer.Resolve<ITurnService>()?.Clear();
			_levelContainer.Resolve<IUnitService>()?.Clear();

			// Destroy level container
			if (_levelContainer)
			{
				_levelContainer.Clear();
				Destroy(_levelContainer.gameObject);
				_levelContainer = null;
			}

			this.Log("Level unloaded successfully.");
			this.Log("====================================", false);
		}

		private IEnumerator LoadLevelCoroutine()
		{
			this.Log("====================================", false);
			this.Log("Loading level...");

			yield return CreateLevelContainer();
			yield return RegisterServices();
			yield return InitializeMap();
			yield return CreateUnits();
			yield return InitializeUIPresenter();
			yield return InitializeInteractionController();
			yield return InitializeInputService();

			this.Log($"Level '{levelConfig.levelName}' loaded successfully!");
			this.Log("====================================", false);

			_levelContainer.Resolve<IGameServer>().StartGame();
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

			_levelContainer.Services.Register<IMapService, MapService>();
			_levelContainer.Services.Register<IUnitService, UnitService>();
			_levelContainer.Services.Register<ITurnService, TurnService>();
			_levelContainer.Services.Register<IGameServer, GameServer>();
			_levelContainer.Services.Register<IPathFindingService>(container =>
			{
				var mapService = container.Resolve<IMapService>();
				var unitService = container.Resolve<IUnitService>();
				var traversalRule = new DefaultTraversalRule(unitService);
				return new PathFindingService(mapService, traversalRule);
			});

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
			_levelContainer.Resolve<IMapService>().LoadFromConfig(levelConfig.mapConfig);
            
			this.Log($"✓ Map initialized: {levelConfig.mapConfig.MapName} {levelConfig.mapConfig.Size.x}x{levelConfig.mapConfig.Size.y})");
			yield return null;
		}

		private IEnumerator CreateUnits()
		{
			this.Log("Spawning units...");

			var mapService = _levelContainer.Resolve<IMapService>();
			var unitService = _levelContainer.Resolve<IUnitService>();

			if (levelConfig.unitPlacements.Count == 0)
			{
				this.LogWarning("No units to spawn!");
				yield break;
			}
            List<Unit> unitsToRender = new List<Unit>();
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
					if (!mapService.Data.IsInBounds(placement.startPosition))
					{
						this.LogError($"Unit '{placement.unitId}' spawn position {placement.startPosition} is out of bounds!");
						continue;
					}

					// Create unit through UnitService
					var unit = unitService.CreateUnit(
						placement.unitId,
						placement.unitConfig,
						placement.startPosition
					);
                    unitsToRender.Add(unit);
					// Occupy cell on map
					mapService.OccupyCell(placement.startPosition, placement.unitId);

					this.Log($"✓ Spawned {unit.name} at {placement.startPosition}");
				}
				catch (Exception ex)
				{
					this.LogError($"Failed to spawn unit '{placement.unitId}': {ex.Message}");
				}

				yield return null;
			}
            _eventBus.Publish(new MapViewRenderUnitEvent(unitsToRender));
			this.Log($"✓ Spawned {levelConfig.unitPlacements.Count} units.");
		}

		private IEnumerator InitializeUIPresenter()
		{
			this.Log("Initializing UI Presenter...");

			var uiManager = RootContainer.Instance.TryResolve<UIManager>();
			if (!uiManager)
			{
				this.LogWarning("No UIManager found in RootContainer!");
				yield break;
			}
			_levelContainer.Services.RegisterInstance(new ActionMenuPresenter(uiManager, _eventBus));

			this.Log("✓ UI Presenter initialized.");
			yield return null;
		}

		private IEnumerator InitializeInteractionController()
		{
			this.Log("Initializing InteractionController...");

			if (!interactionController)
			{
				interactionController = FindObjectOfType<InteractionController>();
				if (!interactionController)
				{
					this.LogWarning("No InteractionController found in scene!");
					yield break;
				}
			}

			var commandQueue = RootContainer.Instance.TryResolve<ICommandQueue>();
			if (commandQueue == null)
			{
				this.LogWarning("No ICommandQueue found in RootContainer!");
				yield break;
			}

			var pathfinding = _levelContainer.Resolve<IPathFindingService>();
			if (pathfinding == null)
			{
				this.LogWarning("No IPathFindingService found in LevelContainer!");
				yield break;
			}

			interactionController.Initialize(
				_eventBus,
				_levelContainer.Resolve<IUnitService>(),
				_levelContainer.Resolve<IMapService>(),
				_levelContainer.Resolve<ITurnService>(),
				commandQueue,
				pathfinding
			);
			this.Log("✓ InteractionController initialized.");
			yield return null;
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
				_levelContainer.Resolve<IMapService>(),
				_levelContainer.Resolve<IUnitService>()
			);
			this.Log("✓ InputService initialized.");
			yield return null;
		}
	}
}

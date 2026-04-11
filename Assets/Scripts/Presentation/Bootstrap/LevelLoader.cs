using System;
using System.Collections;
using System.Collections.Generic;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Config;
using Data.Runtime.Events;
using Presentation.CameraControl;
using Presentation.Input;
using Presentation.Interaction;
using Presentation.Map;
using Presentation.UI.Core;
using Presentation.UI.Presenter;
using Presentation.Unit;
using Sirenix.OdinInspector;
using Systems.AI;
using Systems.Buff;
using Systems.Damage;
using Systems.GamePlay;
using Systems.Interfaces;
using Systems.Map;
using Systems.Map.Region;
using Systems.PathFinding;
using Systems.PathFinding.TraversalRule;
using Systems.Turn;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;

namespace Presentation.Bootstrap
{
	public class LevelLoader : MonoBehaviour
	{
		private struct LoadStepEntry
		{
			public string Desc;
			public Action Execute;
		}

		[Title("References")]
		[SerializeField, Required] private Grid grid;
		[SerializeField, Required] private InputService inputService;
		[SerializeField, Required] private InteractionController interactionController;
		[SerializeField, Required] private UnitViewManager unitViewManager;
		[SerializeField, Required] private CameraController cameraController;

		[Title("Configuration")]
		[SerializeField, Required] private LevelConfig levelConfig;
		[SerializeField] private bool autoLoadLevel;

		private LevelContainer _levelContainer;
		private IEventBus _eventBus;

		private readonly List<LoadStepEntry> _steps = new();

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
			BuildSteps();
			RunAllSteps();
		}

		public void UnloadLevel()
		{
			if (!_levelContainer) return;

			this.Log("====================================", false);
			this.Log("Unloading level...");

			_levelContainer.TryResolve<ITurnService>()?.Clear();
			_levelContainer.TryResolve<IUnitService>()?.Clear();

			_levelContainer.Clear();
			Destroy(_levelContainer.gameObject);
			_levelContainer = null;

			_loadStatus = "Idle";
			this.Log("Level unloaded.");
			this.Log("====================================", false);
		}

		private void BuildSteps()
		{
			_steps.Clear();
            
			AddStep("SetupLevelContainer", SetupLevelContainer);
			AddStep("RegisterServices", RegisterServices);
			AddStep("InitializeComponents", InitializeComponents);
			AddStep("RegisterPresenter", RegisterPresenter);
			AddStep("LoadMap", LoadMap);
			AddStep("InitializeCameraController", InitializeCameraController);
			AddStep("LoadUnits", LoadUnits);
			AddStep("StartGame", StartGame);
		}

		private void AddStep(string desc, Action execute) => _steps.Add(new LoadStepEntry { Desc = desc, Execute = execute });

		private void RunAllSteps()
		{
			_stepResults.Clear();
			_loadStatus = "Loading...";
			this.Log("============ Level Loading ============", false);
			foreach (var step in _steps)
			{
				step.Execute();
				var result = $"SUCCESS: {step.Desc}";
				_stepResults.Add(result);
				this.Log($"<color=#{ColorUtility.ToHtmlStringRGB(Color.yellow)}>{result}</color>", false);
			}
			_loadStatus = "Loaded";
			this.Log($"============ Level Loaded ============", false);
		}

		private void SetupLevelContainer()
		{
			var containerObj = new GameObject("LevelContainer");
			_levelContainer = containerObj.AddComponent<LevelContainer>();
			_levelContainer.Initialize();
		}

		private void RegisterServices()
		{
			_eventBus = RootContainer.Instance.Resolve<IEventBus>();

			var coordinateConverter = new CoordinateConverter(grid);
			_levelContainer.Services.RegisterInstance<ICoordinateConverter>(coordinateConverter);
			_levelContainer.Services.Register<IMapService, MapService>();
			_levelContainer.Services.Register<IUnitService, UnitService>();
			_levelContainer.Services.Register<ITurnService, TurnService>();
			_levelContainer.Services.Register<IGameServer, GameServer>();
            _levelContainer.Services.Register<IDamageService, DamageService>();
            // _levelContainer.Services.Resolve<IDamageService>();
			_levelContainer.Services.Register<IPathFindingService>(container =>
			{
				var mapService = container.Resolve<IMapService>();
				var unitService = container.Resolve<IUnitService>();
				var traversalRule = new DefaultTraversalRule(unitService);
				return new PathFindingService(mapService, traversalRule);
			});
			_levelContainer.Services.Register<IVisionService, VisionService>();
			_levelContainer.Services.Register<IRegionService, RegionService>();
			_levelContainer.Services.Register<IAIService, AIService>();
			_levelContainer.Services.Register<IBuffService, BuffService>();
			_levelContainer.Services.Resolve<IBuffService>();
			_levelContainer.Services.RegisterInstance(interactionController);
		}
        
        private void InitializeComponents()
        {
            inputService.Initialize(_levelContainer.Services);
            interactionController.Initialize(_levelContainer.Services);
            unitViewManager.Initialize(_levelContainer.Services);
        }

        private void RegisterPresenter()
        {
            var uiManager = RootContainer.Instance.Resolve<UIManager>();
            var turnService = _levelContainer.Services.Resolve<ITurnService>();
            var unitService = _levelContainer.Services.Resolve<IUnitService>();
            var coordinateConverter = _levelContainer.Services.Resolve<ICoordinateConverter>();
            var damageServer = _levelContainer.Services.Resolve<IDamageService>();

            _levelContainer.Services.RegisterInstance(new ActionMenuPresenter(uiManager, _eventBus, damageServer,
                unitService, interactionController.Context));
            _levelContainer.Services.RegisterInstance(new TurnBannerPresenter(uiManager, _eventBus, unitService));
            _levelContainer.Services.RegisterInstance(new TurnOrderPresenter(uiManager, _eventBus, turnService,
                unitService));
            _levelContainer.Services.RegisterInstance(new UnitInfoPresenter(uiManager, _eventBus, unitService));
            _levelContainer.Services.RegisterInstance(new CommonPanelPresenter(uiManager, _eventBus,
                coordinateConverter,
                unitService, unitViewManager));
        }

        private void LoadMap()
		{
			if (!levelConfig.mapConfig)
				throw new InvalidOperationException("LevelConfig has no MapConfig assigned!");

			_levelContainer.Resolve<IRegionService>().Initialize(levelConfig.mapConfig);

			_levelContainer.Resolve<IMapService>().LoadFromConfig(levelConfig.mapConfig);
			this.Log($"Map: {levelConfig.mapConfig.MapName} {levelConfig.mapConfig.Size.x}x{levelConfig.mapConfig.Size.y})");
		}

		private void InitializeCameraController() // 需要在LoadMao之后
		{
			cameraController.Initialize(_levelContainer.Services);
		}

		private void LoadUnits()
		{
			var mapService = _levelContainer.Resolve<IMapService>();
			var unitService = _levelContainer.Resolve<IUnitService>();

			if (levelConfig.unitPlacements.Count == 0)
			{
				this.LogWarning("No unit placements defined in LevelConfig.");
				return;
			}

			int spawned = 0;
			foreach (var placement in levelConfig.unitPlacements)
			{
				if (!placement.unitConfig)
				{
					this.LogWarning($"Skipping unit '{placement.unitId}': no UnitConfig assigned.");
					continue;
				}

				if (!mapService.Data.IsInBounds(placement.startPosition))
				{
					this.LogWarning($"Skipping unit '{placement.unitId}': position {placement.startPosition} out of bounds.");
					continue;
				}

				unitService.CreateUnit(
					placement.unitId,
					placement.unitConfig,
					placement.startPosition);

				mapService.OccupyCell(placement.startPosition, placement.unitId);
				spawned++;
			}

			this.Log($"Spawned {spawned}/{levelConfig.unitPlacements.Count} units.");
		}

		private void StartGame()
		{
			_levelContainer.Resolve<IGameServer>().StartGame();
			_eventBus.Publish(new LevelLoadedEvent(levelConfig.levelId, levelConfig.levelName));
		}

		#region Debug

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly, LabelText("Status")]
		private string _loadStatus = "Idle";

		[ShowInInspector, ReadOnly, LabelText("Steps")]
		[ListDrawerSettings(IsReadOnly = true)]
		private readonly List<string> _stepResults = new();

		#endregion
	}
}

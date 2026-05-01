using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data;
using Data.Config;
using Data.Runtime.Events;
using Presentation.AreaEffect;
using Presentation.CameraControl;
using Presentation.Input;
using Presentation.Interaction;
using Presentation.Map;
using Presentation.UI.Core;
using Presentation.UI.Presenter;
using Presentation.Unit;
using Presentation.Vfx;
using Presentation.Vision;
using Sirenix.OdinInspector;
using Systems.AI;
using Systems.AI.Alert;
using Systems.AI.Blackboard;
using Systems.AreaEffect;
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
		[SerializeField, Required] private SpottedEnemyMarkerView spottedEnemyMarkerView;
		[SerializeField, Required] private AreaEffectView areaEffectView;
		[SerializeField, Required] private OneShotVfxView oneShotVfxView;

		[Title("Configuration")]
		[SerializeField, Required] private LevelConfig levelConfig;

		[Title("Lifecycle")]
		[SerializeField]
		private bool autoStartGame = true;

		private LevelContainer _levelContainer;
		private IEventBus _eventBus;

		private readonly List<LoadStepEntry> _steps = new();

		private void Awake()
		{
			var dataManager = RootContainer.Instance
				? RootContainer.Instance.TryResolve<DataManager>()
				: null;

			if (dataManager && dataManager.SelectedLevel)
			{
				LoadLevel(dataManager.SelectedLevel);
				return;
			}

			if (levelConfig)
			{
				LoadLevel(levelConfig);
				return;
			}

			this.LogWarning("No level to load: SelectedLevel is null and autoLoadLevel is false (or levelConfig empty).");
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

			this.Log("====================================", format: false);
			this.Log("Unloading level...");

			_levelContainer.TryResolve<ITurnService>()?.Clear();
			_levelContainer.TryResolve<IUnitService>()?.Clear();

			_levelContainer.Clear();
			Destroy(_levelContainer.gameObject);
			_levelContainer = null;

			this.Log("Level unloaded.");
			this.Log("====================================", format: false);
		}

		private void StartGame()
		{
			if (!_levelContainer)
			{
				this.LogError("Cannot StartGame: level not loaded yet.");
				return;
			}

			inputService.SetEnabled(true);
			_levelContainer.Resolve<IGameServer>().StartGame();
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
			AddStep("InitializeVision", InitializeVision);
		}

		private void AddStep(string desc, Action execute) => _steps.Add(new LoadStepEntry { Desc = desc, Execute = execute });

		private void RunAllSteps()
		{
			_stepResults.Clear();
			this.Log("============ Level Loading ============", format: false);
			foreach (var step in _steps)
			{
				step.Execute();
				var result = $"SUCCESS: {step.Desc}";
				_stepResults.Add(result);
				this.Log($"<color=#{ColorUtility.ToHtmlStringRGB(Color.yellow)}>{result}</color>", format: false);
			}
			this.Log("============ Level Loaded ============", format: false);

			_eventBus.Publish(new LevelLoadedEvent(levelConfig.levelId, levelConfig.levelName));

			if (autoStartGame)
				StartGame();
			else
				this.Log("autoStartGame=false: waiting for external StartGame() call (e.g. cutscene system).");
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
			_levelContainer.Services.Register<IPathFindingService>(container =>
			{
				var mapService = container.Resolve<IMapService>();
				var unitService = container.Resolve<IUnitService>();
				var traversalRule = new DefaultTraversalRule(unitService);
				return new PathFindingService(mapService, traversalRule);
			});
			_levelContainer.Services.Register<IVisionCalculator, VisionCalculator>();
			_levelContainer.Services.Register<IVisionService, VisionService>();
			_levelContainer.Services.Register<IRegionService, RegionService>();
			_levelContainer.Services.Register<IBuffService, BuffService>();
			_levelContainer.Services.Register<IAreaEffectService, AreaEffectService>();

			var unitService = _levelContainer.Resolve<IUnitService>();
			var turnService = _levelContainer.Resolve<ITurnService>();
			var visionCalculator = _levelContainer.Resolve<IVisionCalculator>();
			var regionService = _levelContainer.Resolve<IRegionService>();
			IAIBlackboardService aiBlackboardService = new AIBlackboardService(_eventBus, unitService, turnService, visionCalculator);
			_levelContainer.Services.RegisterInstance(aiBlackboardService);

			IAlertService alertService = new AlertService(_eventBus, unitService, visionCalculator, regionService, aiBlackboardService);
			_levelContainer.Services.RegisterInstance(alertService);
			_levelContainer.Services.Register<IAIService, AIService>();

			_levelContainer.Services.RegisterInstance(interactionController);
			_levelContainer.Services.RegisterInstance(unitViewManager);
			_levelContainer.Services.RegisterInstance(inputService);
		}
        
        private void InitializeComponents()
        {
            inputService.Initialize(_levelContainer.Services);
            interactionController.Initialize(_levelContainer.Services);
            unitViewManager.Initialize(_levelContainer.Services);
            spottedEnemyMarkerView.Initialize(_levelContainer.Services);
            areaEffectView.Initialize(_levelContainer.Services);
            oneShotVfxView.Initialize(_levelContainer.Services);
        }

        private void RegisterPresenter()
        {
            var uiManager = RootContainer.Instance.Resolve<UIManager>();
            var turnService = _levelContainer.Services.Resolve<ITurnService>();
            var unitService = _levelContainer.Services.Resolve<IUnitService>();
            var coordinateConverter = _levelContainer.Services.Resolve<ICoordinateConverter>();
            var damageService = _levelContainer.Services.Resolve<IDamageService>();
            var visionCalculator = _levelContainer.Services.Resolve<IVisionCalculator>();
            var visionService = _levelContainer.Services.Resolve<IVisionService>();

            _levelContainer.Services.RegisterInstance(new ActionMenuPresenter(uiManager, _eventBus));
            _levelContainer.Services.RegisterInstance(new AttackPreviewPresenter(uiManager, _eventBus, damageService, unitService));
            _levelContainer.Services.RegisterInstance(new TurnBannerPresenter(uiManager, _eventBus));
            _levelContainer.Services.RegisterInstance(new TurnOrderPresenter(uiManager, _eventBus, turnService, unitService));
            _levelContainer.Services.RegisterInstance(new UnitInfoPresenter(uiManager, _eventBus, unitService));
            _levelContainer.Services.RegisterInstance(new TargetInfoPresenter(uiManager, _eventBus, unitService, interactionController, visionService));
            _levelContainer.Services.RegisterInstance(new CommonPanelPresenter(uiManager, _eventBus, coordinateConverter, unitService, unitViewManager));
            _levelContainer.Services.RegisterInstance(new GunLineRevealPresenter(_eventBus, visionCalculator, visionService));
            _levelContainer.Services.RegisterInstance(new AbilitySelectionPresenter(uiManager, _eventBus, interactionController));
        }

        private void LoadMap()
		{
			if (!levelConfig.mapConfig)
				throw new InvalidOperationException("LevelConfig has no MapConfig assigned!");

			_levelContainer.Resolve<IRegionService>().Initialize(levelConfig.mapConfig);

			_levelContainer.Resolve<IMapService>().LoadFromConfig(levelConfig.mapConfig);
			this.Log($"Map: {levelConfig.mapConfig.MapName} {levelConfig.mapConfig.Size.x}x{levelConfig.mapConfig.Size.y})");
		}

		private void InitializeCameraController() // 需要在LoadMap之后
			=> cameraController.Initialize(_levelContainer.Services);

		private void LoadUnits()
		{
			var dataManager = RootContainer.Instance.Resolve<DataManager>();
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

				var loadout = dataManager.GetLoadoutFor(placement);
				unitService.CreateUnit(
					placement.unitId,
					placement.unitConfig,
					loadout,
					placement.startPosition,
					placement.patrolWaypoints);

				mapService.OccupyCell(placement.startPosition, placement.unitId);
				spawned++;
			}

			this.Log($"Spawned {spawned}/{levelConfig.unitPlacements.Count} units.");
		}

		private void InitializeVision() => _levelContainer.Resolve<IVisionService>().RecalculateSharedVision();

		#region Debug

		[ShowInInspector, ReadOnly, LabelText("Steps")]
		[ListDrawerSettings(IsReadOnly = true)]
		private readonly List<string> _stepResults = new();

		#endregion
	}
}

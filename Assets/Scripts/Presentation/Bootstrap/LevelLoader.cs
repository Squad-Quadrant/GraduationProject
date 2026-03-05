using System;
using System.Collections;
using System.Collections.Generic;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Config;
using Data.Runtime.Events;
using Presentation.Input;
using Presentation.Interaction;
using Presentation.Map;
using Presentation.UI.Core;
using Presentation.UI.Presenter;
using Presentation.Unit;
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
			AddStep("RegisterPresenter", RegisterPresenter);
			AddStep("InitializeComponents", InitializeComponents);
			AddStep("LoadMap", LoadMap);
			AddStep("LoadUnits", LoadUnits);
			AddStep("StartGame", StartGame);
		}

		private void AddStep(string desc, Action execute) => _steps.Add(new LoadStepEntry { Desc = desc, Execute = execute });

		private void RunAllSteps()
		{
			_stepResults.Clear();
			_loadStatus = "Loading...";
			this.Log("============ Level Loading ============", false);
			for (int i = 0; i < _steps.Count; i++)
			{
				var step = _steps[i];
				try
				{
					step.Execute();
					var result = $"SUCCESS: {step.Desc}";
					_stepResults.Add(result);
					this.Log($"<color=#{ColorUtility.ToHtmlStringRGB(Color.yellow)}>{result}</color>", false);
				}
				catch (Exception ex)
				{
					var result = $"x {step.Desc}: {ex.Message}";
					_stepResults.Add(result);
					this.LogError(result);
					this.LogError(ex.StackTrace);

					_loadStatus = $"Failed at step {i + 1}/{_steps.Count}: {step.Desc}";
					this.LogError("============ Loading Aborted ============", false);
					return;
				}
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
			_levelContainer.Services.Register<IPathFindingService>(container =>
			{
				var mapService = container.Resolve<IMapService>();
				var unitService = container.Resolve<IUnitService>();
				var traversalRule = new DefaultTraversalRule(unitService);
				return new PathFindingService(mapService, traversalRule);
			});
		}

		private void RegisterPresenter()
		{
			var uiManager = RootContainer.Instance.Resolve<UIManager>();
			var turnService = _levelContainer.Services.Resolve<ITurnService>();
			var unitService = _levelContainer.Services.Resolve<IUnitService>();

			_levelContainer.Services.RegisterInstance(new ActionMenuPresenter(uiManager, _eventBus));
			_levelContainer.Services.RegisterInstance(new TurnBannerPresenter(uiManager, _eventBus, unitService));
			_levelContainer.Services.RegisterInstance(new TurnOrderPresenter(uiManager, _eventBus, turnService, unitService));
			_levelContainer.Services.RegisterInstance(new UnitInfoPresenter(uiManager, _eventBus, unitService));
		}

		private void InitializeComponents()
		{
			inputService.Initialize(_levelContainer.Services);
			interactionController.Initialize(_levelContainer.Services);
			unitViewManager.Initialize(_levelContainer.Services);
		}

		private void LoadMap()
		{
			if (!levelConfig.mapConfig)
				throw new InvalidOperationException("LevelConfig has no MapConfig assigned!");

			_levelContainer.Resolve<IMapService>().LoadFromConfig(levelConfig.mapConfig);
			this.Log($"Map: {levelConfig.mapConfig.MapName} {levelConfig.mapConfig.Size.x}x{levelConfig.mapConfig.Size.y})");
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

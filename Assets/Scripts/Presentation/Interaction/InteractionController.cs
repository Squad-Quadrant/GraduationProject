using Core.Commands;
using Core.Events;
using Core.FSM;
using Core.Log;
using Sirenix.OdinInspector;
using Systems.Interaction;
using Systems.Interaction.States;
using Systems.Map;
using Systems.Turn;
using Systems.Unit;
using UnityEngine;

namespace Presentation.Interaction
{
	public class InteractionController : MonoBehaviour
	{
		[Title("Settings")]
		[SerializeField] private bool enableLogs = true;
		[InfoBox("If enabled, the interaction system will start automatically after initialization.", InfoMessageType.None)]
		[SerializeField] private bool autoStart = true;

		[Title("Info")]
		[SerializeField] [ReadOnly] private InteractionContext context;
		[SerializeField] [ReadOnly] private bool isInitialized;
		[SerializeField] [ReadOnly] private bool isRunning;

		public StateMachine<InteractionContext> StateMachine { get; private set; }

		public InteractionContext Context => context;
		public bool IsRunning => isRunning;
		public string CurrentStateName => StateMachine?.CurrentState?.Name ?? "Not Initialized";

		private void Update()
		{
			if (!isInitialized || !isRunning || StateMachine == null || context == null)
				return;

			StateMachine.Update(Time.deltaTime);
		}

		private void OnDestroy()
		{
			StopInteraction();
			StateMachine = null;
			context = null;
			isInitialized = false;
		}


		public void Initialize(
			IEventBus eventBus,
			IUnitService unitService,
			IMapService mapService,
			ITurnService turnService,
			ICommandQueue commandQueue,
			ILog logger)
		{
			if (isInitialized)
			{
				LogWarning("[InteractionController] Already initialized");
				return;
			}

			context = new InteractionContext(
				eventBus,
				unitService,
				mapService,
				turnService,
				commandQueue);

			StateMachine = new StateMachine<InteractionContext>(
				context,
				enableLogs ? logger : null,
				eventBus,
				"InteractionStateMachine");

			context.StateMachine = StateMachine;

			isInitialized = true;
			Log("[InteractionController] Initialized");

			if (autoStart) StartInteraction();
		}

		public void StartInteraction()
		{
			if (!isInitialized)
			{
				LogWarning("[InteractionController] Cannot start - not initialized");
				return;
			}

			if (isRunning)
			{
				LogWarning("[InteractionController] Already running");
				return;
			}

			StateMachine.ChangeState<IdleState>();
			isRunning = true;
			Log("[InteractionController] Interaction started");
		}

		public void StopInteraction()
		{
			StateMachine?.Clear();
			context?.ClearSelection();
			isRunning = false;
			Log("[InteractionController] Interaction stopped");
		}

		public void Pause()
		{
			if (!isInitialized)
			{
				LogWarning("[InteractionController] Cannot pause - not initialized");
				return;
			}

			isRunning = false;
			Log("[InteractionController] Interaction paused");
		}

		public void Resume()
		{
			if (!isInitialized)
			{
				LogWarning("[InteractionController] Cannot resume - not initialized");
				return;
			}

			isRunning = true;
			Log("[InteractionController] Interaction resumed");
		}

		#region Debug

		private void Log(string message)
		{
			if (enableLogs) Debug.Log($"{message}");
		}

		private void LogWarning(string message)
		{
			if (enableLogs) Debug.LogWarning($"{message}");
		}

		#endregion
	}
}

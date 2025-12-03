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
		[SerializeField] private bool autoStart = true;

		[Title("Info")]
		[ShowInInspector, ReadOnly] private InteractionContext _context;
		[ShowInInspector, ReadOnly] private bool _isInitialized;
		[ShowInInspector, ReadOnly] private bool _isRunning;

		public StateMachine<InteractionContext> StateMachine { get; private set; }

		public InteractionContext Context => _context;
		public bool IsRunning => _isRunning;
		public string CurrentStateName => StateMachine?.CurrentState?.Name ?? "Not Initialized";

		private void Update()
		{
			if (!_isInitialized || !_isRunning || StateMachine == null || _context == null)
				return;

			StateMachine.Update(Time.deltaTime);
		}

		private void OnDestroy()
		{
			StopInteraction();
			StateMachine = null;
			_context = null;
			_isInitialized = false;
		}


		public void Initialize(
			IEventBus eventBus,
			IUnitService unitService,
			IMapService mapService,
			ITurnService turnService,
			ICommandQueue commandQueue,
			ILog logger)
		{
			if (_isInitialized)
			{
				LogWarning("[InteractionController] Already initialized");
				return;
			}

			_context = new InteractionContext(
				eventBus,
				unitService,
				mapService,
				turnService,
				commandQueue);

			StateMachine = new StateMachine<InteractionContext>(
				_context,
				enableLogs ? logger : null,
				eventBus,
				"InteractionStateMachine");

			_context.StateMachine = StateMachine;

			_isInitialized = true;
			Log("[InteractionController] Initialized");

			if (autoStart) StartInteraction();
		}

		public void StartInteraction()
		{
			if (!_isInitialized)
			{
				LogWarning("[InteractionController] Cannot start - not initialized");
				return;
			}

			if (_isRunning)
			{
				LogWarning("[InteractionController] Already running");
				return;
			}

			StateMachine.ChangeState<IdleState>();
			_isRunning = true;
			Log("[InteractionController] Interaction started");
		}

		public void StopInteraction()
		{
			StateMachine?.Clear();
			_context?.ClearSelection();
			_isRunning = false;
			Log("[InteractionController] Interaction stopped");
		}

		public void Pause()
		{
			if (!_isInitialized)
			{
				LogWarning("[InteractionController] Cannot pause - not initialized");
				return;
			}

			_isRunning = false;
			Log("[InteractionController] Interaction paused");
		}

		public void Resume()
		{
			if (!_isInitialized)
			{
				LogWarning("[InteractionController] Cannot resume - not initialized");
				return;
			}

			_isRunning = true;
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

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
			ICommandQueue commandQueue)
		{
			if (_isInitialized)
			{
				this.LogWarning("Already initialized");
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
				eventBus,
				"InteractionStateMachine");

			_context.StateMachine = StateMachine;

			_isInitialized = true;
			this.Log("Initialized");

			if (autoStart) StartInteraction();
		}

		public void StartInteraction()
		{
			if (!_isInitialized)
			{
				this.LogWarning("Cannot start - not initialized");
				return;
			}

			if (_isRunning)
			{
				this.LogWarning("Already running");
				return;
			}

			StateMachine.ChangeState<IdleState>();
			_isRunning = true;
			this.Log("Interaction started");
		}

		public void StopInteraction()
		{
			StateMachine?.Clear();
			_context?.ClearSelection();
			_isRunning = false;
			this.Log("Interaction stopped");
		}

		public void Pause()
		{
			if (!_isInitialized)
			{
				this.LogWarning("Cannot pause - not initialized");
				return;
			}

			_isRunning = false;
			this.Log("Interaction paused");
		}

		public void Resume()
		{
			if (!_isInitialized)
			{
				this.LogWarning("Cannot resume - not initialized");
				return;
			}

			_isRunning = true;
			this.Log("Interaction resumed");
		}
	}
}

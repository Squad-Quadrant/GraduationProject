using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;

namespace Core.FSM
{
	public class StateMachine<TContext>
	{
		#region Properties

		/// <summary>
		/// Just for debugging purposes.
		/// </summary>
		public string Name { get; }

		public IState<TContext> CurrentState { get; private set; }

		public IState<TContext> PreviousState { get; private set; }

		public TContext Context { get; } // Context associated with this state machine

		public bool IsTransitioning { get; private set; }

		/// <summary>
		/// If true, the state machine will automatically evaluate and perform transitions during the Update call.
		/// </summary>
		public bool EnableAutoTransitions { get; set; } = true;

		/// <summary>
		/// List of all transitions in the state machine.(Read-only)
		/// </summary>
		public IReadOnlyList<ITransition<TContext>> Transitions => _transitions.AsReadOnly();

		/// <summary>
		/// List of state history.(Read-only)
		/// </summary>
		public IReadOnlyList<string> StateHistory => _stateHistory.AsReadOnly();

		#endregion


		#region Field

		private readonly ILog _logger;
		private readonly IEventBus _eventBus;
		private readonly Dictionary<Type, IState<TContext>> _stateCache = new();
		private readonly List<ITransition<TContext>> _transitions = new();
		private readonly List<string> _stateHistory = new();
		private const int MaxHistorySize = 20;

		#endregion


		#region Constructor

		/// <param name="context">Context Instance</param>
		/// <param name="logger">Logger Instance, optional; used for debugging</param>
		/// <param name="eventBus">EventBus Instance, optional; used for broadcasting state change events</param>
		/// <param name="name"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public StateMachine(TContext context, ILog logger = null, IEventBus eventBus = null, string name = null)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger;
			_eventBus = eventBus;
			Name = name ?? "StateMachine";

			Log($"[FSM] {GetType().Name} Initialized");
		}

		#endregion


		#region State Management

		/// <summary>
		/// Change state by providing a new state instance.
		/// </summary>
		/// <param name="newState">New state instance</param>
		/// <param name="forceTransition">If true, IsTransitioning flag will be ignored</param>
		public void ChangeState(IState<TContext> newState, bool forceTransition = false)
		{
			if (newState == null)
				throw new ArgumentNullException(nameof(newState));

			if (!forceTransition && IsTransitioning)
			{
				LogWarning($"[FSM] [{Name}] Cannot change state while transitioning. Current: {CurrentState?.Name}, Requested: {newState.Name}");
				return;
			}

			if (CurrentState == newState)
			{
				LogWarning($"[FSM] [{Name}] Already in state {newState.Name}, ignoring change request.");
				return;
			}

			IsTransitioning = true;

			try
			{
				if (CurrentState != null)
				{
					Log($"[FSM] [{Name}] Exiting state: {CurrentState.Name}");
					CurrentState.OnExit(Context);
				}

				PreviousState = CurrentState;
				CurrentState = newState;

				AddToHistory(CurrentState.Name);

				Log($"[FSM] [{Name}] Entering state: {CurrentState.Name}");
				CurrentState.OnEnter(Context);

				PublishStateChangedEvent();

				Log($"[FSM] [{Name}] State changed: {PreviousState?.Name ?? "None"} -> {CurrentState.Name}");
			}
			catch (Exception ex)
			{
				LogError($"[FSM] [{Name}] Error during state change: {ex.Message}\n{ex.StackTrace}");
				throw;
			}
			finally
			{
				IsTransitioning = false;
			}
		}

		/// <summary>
		/// Change state by type. The state instance will be retrieved from the state cache or created if not present.
		/// </summary>
		/// <typeparam name="TState"></typeparam>
		public void ChangeState<TState>() where TState : IState<TContext>, new()
		{
			var stateType = typeof(TState);

			if (!_stateCache.TryGetValue(stateType, out var state))
			{
				state = new TState();
				_stateCache[stateType] = state;
				Log($"[FSM] [{Name}] Created new state instance: {state.Name}");
			}
			ChangeState(state);
		}

		/// <summary>
		/// Update state. This should be called manually in the main update loop.
		/// </summary>
		public void Update(float deltaTime)
		{
			if (CurrentState == null)
			{
				LogWarning($"[FSM] [{Name}] No current state to update.");
				return;
			}

			if (IsTransitioning) return;

			try
			{
				CurrentState.OnUpdate(Context, deltaTime);

				if (EnableAutoTransitions)
					CheckTransitions();
			}
			catch (Exception ex)
			{
				LogError($"[FSM] [{Name}] Error during state update ({CurrentState.Name}): {ex.Message}\n{ex.StackTrace}");
				throw;
			}
		}

		public void RevertToPreviousState()
		{
			if (PreviousState == null)
			{
				LogWarning($"[FSM] [{Name}] No previous state to revert to.");
				return;
			}

			Log($"[FSM] [{Name}] Reverting to previous state: {PreviousState.Name}");
			ChangeState(PreviousState);
		}

		public void Clear()
		{
			Log($"[FSM] [{Name}] Clearing up...");
			if (CurrentState != null)
			{
				CurrentState.OnExit(Context);
				CurrentState = null;
			}
			PreviousState = null;
			_stateCache.Clear();
			_transitions.Clear();
			_stateHistory.Clear();
			Log($"[FSM] [{Name}] Cleared.");
		}

		public bool IsInState<TState>() where TState : IState<TContext> => CurrentState != null && CurrentState.GetType() == typeof(TState);
		public bool IsInState(IState<TContext> state) => CurrentState == state;

		#endregion


		#region Transition Management

		public void AddTransition(ITransition<TContext> transition)
		{
			if (transition == null)
				throw new ArgumentNullException(nameof(transition));

			if (_transitions.Any(t => t.From == transition.From && t.To == transition.To))
				LogWarning($"[{Name}] Transition already exists: {transition.Name}");

			_transitions.Add(transition);
			_transitions.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Higher priority first
			Log($"[FSM] [{Name}] Added transition: {transition.Name} (Priority: {transition.Priority})");
		}

		public void RemoveTransition(ITransition<TContext> transition)
		{
			if (_transitions.Remove(transition))
				Log($"[FSM] [{Name}] Removed transition: {transition.Name}");
			else
				LogWarning($"[FSM] [{Name}] Transition not found: {transition.Name}");
		}

		public void ClearTransitions()
		{
			_transitions.Clear();
			Log($"[FSM] [{Name}] All transitions cleared.");
		}

		private void CheckTransitions()
		{
			if (_transitions.Count == 0)
				return;

			foreach (var transition in _transitions)
			{
				if (transition.From != null && transition.From != CurrentState)
					continue;

				if (!transition.ShouldTransition(Context))
					continue;

				Log($"[FSM] [{Name}] Auto transition triggered: {transition.Name}");

				try
				{
					transition.OnTransition(Context);
					ChangeState(transition.To);
					break; // Only one transition per update
				}
				catch (Exception ex)
				{
					LogError($"[FSM] [{Name}] Error during transition ({transition.Name}): {ex.Message}\n{ex.StackTrace}");
					throw;
				}
			}
		}

		#endregion


		#region History Management

		private void AddToHistory(string stateName)
		{
			_stateHistory.Add($"[{DateTime.Now:HH:mm:ss}] {stateName}");

			if (_stateHistory.Count > MaxHistorySize)
				_stateHistory.RemoveAt(0);
		}

		public void ClearHistory() => _stateHistory.Clear();

		#endregion


		#region Events

		private void PublishStateChangedEvent()
		{
			if (_eventBus == null) return;

			var stateChangedEvent = new StateChangedEvent<TContext>(this, PreviousState, CurrentState, Context);
			_eventBus.Publish(stateChangedEvent);
		}

		#endregion


		#region Logging

		private void Log(string message) => _logger?.Log(message);
		private void LogWarning(string message) => _logger?.LogWarning(message);
		private void LogError(string message) => _logger?.LogError(message);

		#endregion
	}
}

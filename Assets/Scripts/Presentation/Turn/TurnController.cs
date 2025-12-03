using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Data.Runtime.Events.Turn;
using Sirenix.OdinInspector;
using Systems.Turn;
using Systems.Unit;
using UnityEngine;

namespace Presentation.Turn
{
	public class TurnController : MonoBehaviour
	{
		[Title("Settings")]
		[SerializeField] private bool autoStart = true;
		[SerializeField] private bool enableLogs = true;

		[Tooltip("Delay before starting a new turn")]
		[SerializeField, Range(0f, 2f)] private float turnStartDelay = 0.5f;

		[Tooltip("Delay before switching to next unit")]
		[SerializeField, Range(0f, 1f)] private float unitSwitchDelay = 0.2f;


		[Title("Runtime State")]
		[ShowInInspector, ReadOnly] public bool IsRunning { get; private set; }
		[ShowInInspector, ReadOnly] private bool _isInitialized;

#if UNITY_EDITOR
		[ShowInInspector, ReadOnly] public int CurrentTurnNumber => _turnService?.GetCurrentTurnNumber() ?? 0;
		[ShowInInspector, ReadOnly] public string CurrentUnitId => _turnService?.GetCurrentUnit()?.Id ?? "None";
		[ShowInInspector, ReadOnly] public int QueueCount => _turnService?.GetRemainingUnitsCount() ?? 0;
		[ShowInInspector, ReadOnly] public IReadOnlyList<ITurnUnit> TurnOrder => _turnService?.GetTurnOrder() ?? new List<ITurnUnit>();
#endif
		private ITurnService _turnService;
		private IUnitService _unitService;
		private IEventBus _eventBus;


		public void Initialize(ITurnService turnService, IUnitService unitService, IEventBus eventBus)
		{
			if (_isInitialized)
			{
				LogWarning("Already initialized!");
				return;
			}

			_turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
			_unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

			_eventBus.Subscribe<UnitTurnEndedEvent>(OnUnitTurnEnded);
			_eventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);

			_isInitialized = true;
			Log("Initialized");

			if (autoStart)
				StartTurnCycle();
		}

		[TitleGroup("Controls")]
		[HorizontalGroup("Controls/Buttons")]
		[Button("Start"), GUIColor(0.4f, 1f, 0.4f)]
		[EnableIf("@UnityEngine.Application.isPlaying && _isInitialized && !IsRunning")]
		public void StartTurnCycle()
		{
			if (!_isInitialized)
			{
				LogError("Not initialized!");
				return;
			}

			if (IsRunning)
			{
				LogWarning("Already running!");
				return;
			}

			if (_unitService.GetAllAliveUnits().Count == 0)
			{
				LogError("No units available!");
				return;
			}

			IsRunning = true;
			Log("Turn cycle started");

			_turnService.StartTurn();
		}

		[HorizontalGroup("Controls/Buttons")]
		[Button("Stop"), GUIColor(1f, 0.4f, 0.4f)]
		[EnableIf("@UnityEngine.Application.isPlaying && IsRunning")]
		public void StopTurnCycle()
		{
			if (!IsRunning) return;

			StopAllCoroutines();

			if (_turnService.IsTurnActive())
				_turnService.EndTurn();

			IsRunning = false;
			Log("Turn cycle stopped");
		}

		[HorizontalGroup("Controls/Buttons")]
		[Button("Pause"), GUIColor(1f, 1f, 0.4f)]
		[EnableIf("@UnityEngine.Application.isPlaying && IsRunning")]
		public void Pause()
		{
			StopAllCoroutines();
			Log("Paused");
		}

		private void OnUnitTurnEnded(UnitTurnEndedEvent e)
		{
			if (!IsRunning) return;

			Log($"Unit '{e.UnitId}' turn ended");

			if (_turnService.IsCurrentTurnComplete())
			{
				Log("Queue empty → EndTurn");
				StartCoroutine(DelayedAction(unitSwitchDelay, () => _turnService.EndTurn()));
			}
			else
			{
				Log($"Queue has {QueueCount} units → NextUnit");
				StartCoroutine(DelayedAction(unitSwitchDelay, () => _turnService.NextUnit()));
			}
		}

		private void OnTurnEnded(TurnEndedEvent e)
		{
			if (!IsRunning) return;

			Log($"Turn {e.TurnNumber} ended → StartTurn");
			StartCoroutine(DelayedAction(turnStartDelay, () => _turnService.StartTurn()));
		}

		private IEnumerator DelayedAction(float delay, Action action)
		{
			if (delay > 0)
				yield return new WaitForSeconds(delay);

			if (IsRunning)
				action?.Invoke();
		}

		#region Debug

		[TitleGroup("Debug")]
		[HorizontalGroup("Debug/Buttons")]
		[Button("Force Next")]
		[EnableIf("@UnityEngine.Application.isPlaying && IsRunning")]
		private void DebugForceNext()
		{
			if (_turnService.HasCurrentUnit())
				_turnService.EndUnitTurn();
		}

		[HorizontalGroup("Debug/Buttons")]
		[Button("Log Queue")]
		[EnableIf("@UnityEngine.Application.isPlaying && _isInitialized")]
		private void DebugLogQueue()
		{
			var order = _turnService.GetTurnOrder();
			var str = string.Join(" → ", order.Select(u => $"{u.Id}(S:{u.Speed})"));
			Debug.Log($"[TurnController] Queue: {str}");
		}

		private void Log(string msg)
		{
			if (enableLogs) Debug.Log($"[TurnController] {msg}");
		}

		private void LogWarning(string msg)
		{
			if (enableLogs) Debug.LogWarning($"[TurnController] {msg}");
		}

		private void LogError(string msg)
		{
			Debug.LogError($"[TurnController] {msg}");
		}

		#endregion
	}
}

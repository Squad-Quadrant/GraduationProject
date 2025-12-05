using System.Collections.Generic;
using System.Linq;
using Core.FSM;
using Core.Log;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Debugger
{
	public abstract class StateMachineDebugger : MonoBehaviour
	{
		#region Connection Status

		[TitleGroup("Connection", boldTitle: true)]
		[ShowInInspector, ReadOnly]
		[GUIColor("@GetConnectionColor()")]
		[PropertyOrder(-100)]
		protected abstract bool IsConnected { get; }

		[TitleGroup("Connection")]
		[ShowInInspector, ReadOnly, DisplayAsString]
		[PropertyOrder(-99)]
		[ShowIf("@!IsConnected")]
		private string ConnectionHint => "Waiting for target... (Auto-connect on play)";

		#endregion

		#region Basic Info

		[TitleGroup("Info", boldTitle: true)]
		[HorizontalGroup("Info/Split", 0.5f)]
		[BoxGroup("Info/Split/Basic")]
		[LabelText("FSM"), LabelWidth(90)]
		[DisplayAsString, ShowInInspector]
		[GUIColor(0.3f, 0.8f, 1f)]
		protected abstract string StateMachineName { get; }

		[BoxGroup("Info/Split/Basic")]
		[LabelText("Current"), LabelWidth(90)]
		[DisplayAsString, ShowInInspector]
		[GUIColor("@GetStateColor()")]
		protected abstract string CurrentStateName { get; }

		[BoxGroup("Info/Split/State")]
		[LabelText("Previous"), LabelWidth(90)]
		[DisplayAsString, ShowInInspector]
		[GUIColor(0.7f, 0.7f, 0.7f)]
		protected abstract string PreviousStateName { get; }

		[BoxGroup("Info/Split/State")]
		[LabelText("IsTransitioning"), LabelWidth(90)]
		[DisplayAsString, ShowInInspector]
		[GUIColor("@IsTransitioning ? new Color(1f, 0.5f, 0f) : new Color(0.5f, 0.5f, 0.5f)")]
		protected abstract bool IsTransitioning { get; }

		[BoxGroup("Info/Split/State")]
		[LabelText("AutoTransition"), LabelWidth(90)]
		[DisplayAsString, ShowInInspector]
		[GUIColor("@EnableAutoTransitions ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f)")]
		protected abstract bool EnableAutoTransitions { get; }

		#endregion

		#region State History

		[TitleGroup("State History", "20 Most Recent States")]
		[InfoBox("@GetHistoryInfo()", InfoMessageType.None)]
		[ListDrawerSettings(
			ShowIndexLabels = false,
			ShowPaging = false,
			DraggableItems = false,
			NumberOfItemsPerPage = 10,
			CustomAddFunction = nameof(DummyAdd)
		)]
		[ShowInInspector, ReadOnly]
		[PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
		protected abstract List<string> StateHistory { get; }

		#endregion

		#region Transitions

		[TitleGroup("Registered Transitions", "All Defined State Transitions")]
		[InfoBox("@GetTransitionsInfo()", InfoMessageType.None)]
		[ListDrawerSettings(
			ShowIndexLabels = true,
			ShowPaging = false,
			DraggableItems = false,
			NumberOfItemsPerPage = 10,
			CustomAddFunction = nameof(DummyAdd)
		)]
		[ShowInInspector, ReadOnly]
		[PropertySpace(SpaceBefore = 5, SpaceAfter = 5)]
		protected abstract List<string> Transitions { get; }

		#endregion

		#region Control Panel

		[TitleGroup("Control Panel")]
		[HorizontalGroup("Control Panel/Buttons")]
		[Button("Revert To Previous", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
		[EnableIf("@IsApplicationPlaying && IsConnected && !string.IsNullOrEmpty(PreviousStateName) && PreviousStateName != \"None\"")]
		protected void RevertToPreviousState() => RevertToPreviousStateImpl();

		[HorizontalGroup("Control Panel/Buttons")]
		[Button("Toggle Auto Transitions", ButtonSizes.Large), GUIColor(0.3f, 1f, 0.3f)]
		[EnableIf("@IsApplicationPlaying && IsConnected")]
		protected void ToggleAutoTransitions() => ToggleAutoTransitionsImpl();

		[HorizontalGroup("Control Panel/Buttons")]
		[Button("Clear History", ButtonSizes.Large), GUIColor(1f, 0.4f, 0.4f)]
		[EnableIf("@IsApplicationPlaying && IsConnected")]
		protected void ClearHistory() => ClearHistoryImpl();

		#endregion

		#region Helper Methods

		private bool IsApplicationPlaying => Application.isPlaying;

		private Color GetConnectionColor() =>
			IsConnected ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f);

		private Color GetStateColor()
		{
			if (!IsConnected || string.IsNullOrEmpty(CurrentStateName) || CurrentStateName == "None")
				return new Color(0.5f, 0.5f, 0.5f);

			// Generate consistent color based on state name hash
			var hash = CurrentStateName.GetHashCode();
			var hue = (hash % 360) / 360f;
			return Color.HSVToRGB(hue, 0.6f, 1f);
		}

		private string GetHistoryInfo()
		{
			if (!IsConnected) return "Not connected";
			if (StateHistory == null || StateHistory.Count == 0)
				return "No History Records";
			return $"Total: {StateHistory.Count}";
		}

		private string GetTransitionsInfo()
		{
			if (!IsConnected) return "Not connected";
			if (Transitions == null || Transitions.Count == 0)
				return "No transitions defined";
			return $"Total: {Transitions.Count}";
		}

		private string DummyAdd() => null;

		#endregion

		protected abstract void RevertToPreviousStateImpl();
		protected abstract void ClearHistoryImpl();
		protected abstract void ToggleAutoTransitionsImpl();
	}

	public abstract class StateMachineDebuggerBase<TContext> : StateMachineDebugger
	{
		protected StateMachine<TContext> StateMachine { get; private set; }

		protected virtual void OnEnable()
		{
			// Attempt to find state machine immediately if playing
			if (Application.isPlaying)
				TryConnect();
		}

		protected virtual void Update()
		{
			// Keep trying to connect if not connected yet
			if (Application.isPlaying && StateMachine == null)
				TryConnect();
		}

		protected virtual void OnDisable() => StateMachine = null;

		#region Connection

		private void TryConnect() => StateMachine = FindStateMachine();

		protected abstract StateMachine<TContext> FindStateMachine();

		public void SetStateMachine(StateMachine<TContext> stateMachine) => StateMachine = stateMachine;

		#endregion

		#region Property Implementations

		protected override bool IsConnected => StateMachine != null;
		protected override string StateMachineName => StateMachine?.Name ?? "Not Connected";
		protected override string CurrentStateName => StateMachine?.CurrentState?.Name ?? "None";
		protected override string PreviousStateName => StateMachine?.PreviousState?.Name ?? "None";
		protected override bool IsTransitioning => StateMachine?.IsTransitioning ?? false;
		protected override bool EnableAutoTransitions => StateMachine?.EnableAutoTransitions ?? false;


		protected override List<string> StateHistory
		{
			get
			{
				if (StateMachine == null)
					return new List<string> { "Not connected" };

				var history = StateMachine.StateHistory.ToList();
				if (history.Count == 0)
					return new List<string> { "No history records" };

				// Add arrow icon to make current state more visible
				return history.Select((h, i) =>
				{
					var icon = i == history.Count - 1 ? "▶" : "  ";
					return $"{icon} {h}";
				}).ToList();
			}
		}

		protected override List<string> Transitions
		{
			get
			{
				if (StateMachine == null)
					return new List<string> { "Not connected" };

				var transitions = StateMachine.Transitions
					.Select(t => $"[P:{t.Priority}] {t.Name}")
					.ToList();

				return transitions.Count > 0 ?
					transitions : new List<string> { "No transitions defined" };
			}
		}

		#endregion

		#region Control Implementations

		protected override void RevertToPreviousStateImpl()
		{
			if (StateMachine == null)
			{
				this.LogWarning("Not connected");
				return;
			}

			if (StateMachine.PreviousState == null)
			{
				this.LogWarning("No previous state to revert to");
				return;
			}

			StateMachine.RevertToPreviousState();
		}

		protected override void ClearHistoryImpl()
		{
			if (StateMachine == null)
			{
				this.LogWarning("Not connected");
				return;
			}

			StateMachine.ClearHistory();
		}

		protected override void ToggleAutoTransitionsImpl()
		{
			if (StateMachine == null)
			{
				this.LogWarning("Not connected");
				return;
			}

			StateMachine.EnableAutoTransitions = !StateMachine.EnableAutoTransitions;
		}

		#endregion
	}
}


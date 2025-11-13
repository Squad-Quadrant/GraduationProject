using System.Collections.Generic;
using System.Linq;
using Core.FSM;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.FSM
{
	public abstract class StateMachineDebugger : MonoBehaviour
	{
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

		[TitleGroup("Control Panel")]
		[HorizontalGroup("Control Panel/Buttons")]
		[Button("Revert To Previous", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
		[EnableIf("@IsApplicationPlaying && !string.IsNullOrEmpty(PreviousStateName) && PreviousStateName != \"无\"")]
		protected void RevertToPreviousState() => RevertToPreviousStateImpl();

		private bool IsApplicationPlaying => Application.isPlaying;

		[HorizontalGroup("Control Panel/Buttons")]
		[Button("Toggle Auto Transitions", ButtonSizes.Large), GUIColor(0.3f, 1f, 0.3f)]
		[EnableIf("IsApplicationPlaying")]
		protected void ToggleAutoTransitions() => ToggleAutoTransitionsImpl();

		[HorizontalGroup("Control Panel/Buttons")]
		[Button("Clear History", ButtonSizes.Large), GUIColor(1f, 0.4f, 0.4f)]
		[EnableIf("IsApplicationPlaying")]
		protected void ClearHistory() => ClearHistoryImpl();

		private Color GetStateColor()
		{
			if (string.IsNullOrEmpty(CurrentStateName) || CurrentStateName == "无")
				return new Color(0.5f, 0.5f, 0.5f);

			var hash = CurrentStateName.GetHashCode();
			var hue = (hash % 360) / 360f;
			return Color.HSVToRGB(hue, 0.6f, 1f);
		}

		private string GetHistoryInfo()
		{
			if (StateHistory == null || StateHistory.Count == 0)
				return "No History Records";

			return $"Total: {StateHistory.Count}";
		}

		private string GetTransitionsInfo()
		{
			if (Transitions == null || Transitions.Count == 0)
				return "No Transition Defined";

			return $"Total: {Transitions.Count}";
		}

		private string DummyAdd() => null;

		protected abstract void RevertToPreviousStateImpl();
		protected abstract void ClearHistoryImpl();
		protected abstract void ToggleAutoTransitionsImpl();
	}

	public abstract class StateMachineDebuggerBase<TContext> : StateMachineDebugger
	{
		protected StateMachine<TContext> StateMachine { get; private set; }

		public void SetStateMachine(StateMachine<TContext> stateMachine)
		{
			StateMachine = stateMachine;
		}

		protected override string StateMachineName => StateMachine?.Name ?? "Not Set";
		protected override string CurrentStateName => StateMachine?.CurrentState?.Name ?? "Not Set";
		protected override string PreviousStateName => StateMachine?.PreviousState?.Name ?? "Not Set";
		protected override bool IsTransitioning => StateMachine?.IsTransitioning ?? false;
		protected override bool EnableAutoTransitions => StateMachine?.EnableAutoTransitions ?? false;

		protected override List<string> StateHistory
		{
			get
			{
				if (StateMachine == null)
					return new List<string> { "FSM not set" };

				var history = StateMachine.StateHistory.ToList();
				if (history.Count == 0)
					return new List<string> { "No History Records" };

				// 添加图标让历史记录更易读
				return history.Select((h, index) =>
				{
					var icon = index == history.Count - 1 ? "▶️" : "  ";
					return $"{icon} {h}";
				}).ToList();
			}
		}

		protected override List<string> Transitions
		{
			get
			{
				if (StateMachine == null)
					return new List<string> { "FSM not set" };

				var transitions = StateMachine.Transitions
					.Select(t => $"[P:{t.Priority}] {t.Name}")
					.ToList();

				return transitions.Count > 0 ? transitions : new List<string> { "No Transition Defined" };
			}
		}

		protected override void RevertToPreviousStateImpl()
		{
			if (StateMachine == null)
			{
				Debug.LogWarning("FSM not set; cannot revert to previous state");
				return;
			}

			if (StateMachine.PreviousState == null)
			{
				Debug.LogWarning("No previous state to revert to");
				return;
			}

			StateMachine.RevertToPreviousState();
		}

		protected override void ClearHistoryImpl()
		{
			if (StateMachine == null)
			{
				Debug.LogWarning("FSM not set; cannot clear history");
				return;
			}

			StateMachine.ClearHistory();
		}

		protected override void ToggleAutoTransitionsImpl()
		{
			if (StateMachine == null)
			{
				Debug.LogWarning("FSM not set; cannot toggle auto transitions");
				return;
			}

			StateMachine.EnableAutoTransitions = !StateMachine.EnableAutoTransitions;
		}

		protected virtual void OnDestroy()
		{
			StateMachine = null;
		}
	}
}


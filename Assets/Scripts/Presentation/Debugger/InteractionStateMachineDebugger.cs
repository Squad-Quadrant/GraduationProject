using Core.FSM;
using Presentation.Interaction;
using Sirenix.OdinInspector;
using Systems.Interaction;
using UnityEngine;

namespace Presentation.Debugger
{
	[AddComponentMenu("Debugger/Interaction State Machine Debugger")]
	public class InteractionStateMachineDebugger : StateMachineDebuggerBase<InteractionContext>
	{
		private InteractionController _cachedController;
		private bool HasSelectedUnit => _cachedController?.Context?.selectedUnit != null;

		[TitleGroup("Auto-Find Settings", order: -200)]
		[SerializeField]
		[Tooltip("If assigned, use this controller directly. Otherwise, find in scene.")]
		private InteractionController targetController;

		[TitleGroup("Auto-Find Settings")]
		[ShowInInspector, ReadOnly]
		[InfoBox("@GetControllerStatus()", InfoMessageType.None)]
		private string ControllerName => _cachedController != null ? _cachedController.name : "Not Found";

		[TitleGroup("Interaction Context", order: 100)]
		[ShowInInspector, ReadOnly]
		[LabelText("Selected Unit")]
		[GUIColor("@HasSelectedUnit ? new Color(0.3f, 1f, 0.6f) : new Color(0.5f, 0.5f, 0.5f)")]
		private string SelectedUnitInfo => GetSelectedUnitInfo();

		[TitleGroup("Interaction Context")]
		[ShowInInspector, ReadOnly]
		[LabelText("Target Cell")]
		private string TargetCellInfo => GetTargetCellInfo();

		[TitleGroup("Interaction Context")]
		[ShowInInspector, ReadOnly]
		[LabelText("Current Action")]
		private string CurrentActionInfo => _cachedController?.Context?.currentAction.ToString() ?? "None";

		[TitleGroup("Interaction Context")]
		[ShowInInspector, ReadOnly]
		[LabelText("Valid Targets")]
		private int ValidTargetCount => _cachedController?.Context?.validTargetCells?.Count ?? 0;

		[TitleGroup("Interaction Context")]
		[ShowInInspector, ReadOnly]
		[LabelText("Path Length")]
		private int PathLength => _cachedController?.Context?.currentPath?.Count ?? 0;

		[TitleGroup("Interaction Context")]
		[ShowInInspector, ReadOnly]
		private InteractionContext FullContext => _cachedController?.Context;

		protected override StateMachine<InteractionContext> FindStateMachine()
		{
			if (targetController)
			{
				_cachedController = targetController;
				return targetController.StateMachine;
			}

			_cachedController = FindObjectOfType<InteractionController>();
			return _cachedController?.StateMachine;
		}

		private string GetControllerStatus()
		{
			if (_cachedController == null)
				return "Searching for InteractionController in scene...";

			return _cachedController.IsRunning
				? "Controller found and running"
				: "Controller found but not running";
		}

		private string GetSelectedUnitInfo()
		{
			var unit = _cachedController?.Context?.selectedUnit;
			if (unit == null) return "None";
			return $"{unit.name} ({unit.id}) at {unit.position}";
		}

		private string GetTargetCellInfo()
		{
			var ctx = _cachedController?.Context;
			if (ctx == null) return "None";

			return ctx.targetCell == InteractionContext.InvalidCell ? "None" : ctx.targetCell.ToString();
		}

		#region Debug Actions

		[TitleGroup("Debug Actions", order: 200)]
		[HorizontalGroup("Debug Actions/Row1")]
		[Button("Clear Selection", ButtonSizes.Large), GUIColor(1f, 0.8f, 0.4f)]
		[EnableIf("@IsApplicationPlaying && IsConnected && HasSelectedUnit")]
		private void ClearSelection()
		{
			_cachedController?.Context?.ClearSelection();
			Debug.Log("[InteractionDebugger] Selection cleared");
		}

		[HorizontalGroup("Debug Actions/Row1")]
		[Button("Force Idle", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
		[EnableIf("@IsApplicationPlaying && IsConnected")]
		private void ForceIdle()
		{
			StateMachine?.ChangeState<Systems.Interaction.States.IdleState>();
			Debug.Log("[InteractionDebugger] Forced to Idle state");
		}

		[HorizontalGroup("Debug Actions/Row1")]
		[Button("Pause/Resume", ButtonSizes.Large), GUIColor(0.8f, 0.8f, 0.4f)]
		[EnableIf("@IsApplicationPlaying && _cachedController != null")]
		private void TogglePause()
		{
			if (!_cachedController) return;

			if (_cachedController.IsRunning)
				_cachedController.Pause();
			else
				_cachedController.Resume();

			Debug.Log($"[InteractionDebugger] {(_cachedController.IsRunning ? "Resumed" : "Paused")}");
		}

		#endregion
	}
}

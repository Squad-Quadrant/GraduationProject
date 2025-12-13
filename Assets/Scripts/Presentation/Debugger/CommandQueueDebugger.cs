using System.Collections.Generic;
using System.Linq;
using Core.Commands;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Debugger
{
	[AddComponentMenu("Debugger/Command Queue Debugger")]
	public class CommandQueueDebugger : MonoBehaviour
	{
		#region Connection

		[TitleGroup("Connection", order: -100)]
		[ShowInInspector, ReadOnly]
		[GUIColor("@IsConnected ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f)")]
		private bool IsConnected => _commandQueue != null;

		[TitleGroup("Connection")]
		[ShowInInspector, ReadOnly, DisplayAsString]
		[HideIf("IsConnected")]
		private string ConnectionHint => "Waiting for target...";

		#endregion

		#region Status

		[TitleGroup("Status", boldTitle: true)]
		[HorizontalGroup("Status/Row1")]
		[BoxGroup("Status/Row1/Counts")]
		[ShowInInspector, ReadOnly]
		[LabelText("Pending"), LabelWidth(60)]
		[GUIColor("@PendingCount > 0 ? new Color(1f, 0.8f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
		private int PendingCount => _commandQueue?.PendingCount ?? 0;

		[BoxGroup("Status/Row1/Counts")]
		[ShowInInspector, ReadOnly]
		[LabelText("Undo"), LabelWidth(60)]
		[GUIColor("@UndoCount > 0 ? new Color(0.3f, 0.8f, 1f) : new Color(0.5f, 0.5f, 0.5f)")]
		private int UndoCount => _commandQueue?.UndoCount ?? 0;

		[BoxGroup("Status/Row1/Counts")]
		[ShowInInspector, ReadOnly]
		[LabelText("Redo"), LabelWidth(60)]
		[GUIColor("@RedoCount > 0 ? new Color(0.8f, 0.3f, 1f) : new Color(0.5f, 0.5f, 0.5f)")]
		private int RedoCount => _commandQueue?.RedoCount ?? 0;

		[BoxGroup("Status/Row1/Flags")]
		[ShowInInspector, ReadOnly]
		[LabelText("Executing"), LabelWidth(70)]
		[GUIColor("@IsExecuting ? new Color(0.3f, 1f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
		private bool IsExecuting => _commandQueue?.IsExecuting ?? false;

		[BoxGroup("Status/Row1/Flags")]
		[ShowInInspector, ReadOnly]
		[LabelText("Paused"), LabelWidth(70)]
		[GUIColor("@IsPaused ? new Color(1f, 0.5f, 0f) : new Color(0.5f, 0.5f, 0.5f)")]
		private bool IsPaused => _commandQueue?.IsPaused ?? false;

		[BoxGroup("Status/Row1/Flags")]
		[ShowInInspector, ReadOnly]
		[LabelText("Idle"), LabelWidth(70)]
		[GUIColor("@IsIdle ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.5f, 0.5f, 0.5f)")]
		private bool IsIdle => _commandQueue?.IsIdle ?? true;

		#endregion

		#region Current Command

		[TitleGroup("Current Command", boldTitle: true)]
		[ShowInInspector, ReadOnly]
		[GUIColor("@HasCurrentCommand ? new Color(0.3f, 1f, 0.6f) : new Color(0.5f, 0.5f, 0.5f)")]
		[InfoBox("No command executing", InfoMessageType.None, VisibleIf = "@!HasCurrentCommand")]
		private string CurrentCommandName => _commandQueue?.CurrentCommand?.Name ?? "None";

		private bool HasCurrentCommand => _commandQueue?.CurrentCommand != null;

		#endregion

		#region Pending Commands

		[TitleGroup("Pending Commands", "Commands waiting to be executed")]
		[ShowInInspector, ReadOnly]
		[ListDrawerSettings(
			ShowIndexLabels = true,
			ShowPaging = false,
			DraggableItems = false,
			NumberOfItemsPerPage = 10
		)]
		[InfoBox("Queue is empty", InfoMessageType.None, VisibleIf = "@PendingCommands.Count == 0")]
		private List<string> PendingCommands
		{
			get
			{
				if (_commandQueue == null)
					return new List<string> { "Not connected" };

				var names = _commandQueue.PendingCommandNames;
				if (names == null || names.Count == 0)
					return new List<string>();

				// Add execution order indicator
				return names.Select((n, i) => $"[{i + 1}] {n}").ToList();
			}
		}

		#endregion

		#region Command History

		[TitleGroup("Undo Stack", "Executed commands (can undo)")]
		[ShowInInspector, ReadOnly]
		[ListDrawerSettings(
			ShowIndexLabels = false,
			ShowPaging = false,
			DraggableItems = false,
			NumberOfItemsPerPage = 8
		)]
		[InfoBox("No commands to undo", InfoMessageType.None, VisibleIf = "@UndoStack.Count == 0")]
		private List<string> UndoStack
		{
			get
			{
				if (_commandQueue == null)
					return new List<string> { "Not connected" };

				var names = _commandQueue.ExecutedCommandNames;
				if (names == null || names.Count == 0)
					return new List<string>();

				// Most recent at top
				return names.Select((n, i) => i == 0 ? $"▶ {n}" : $"  {n}").ToList();
			}
		}

		[TitleGroup("Redo Stack", "Undone commands (can redo)")]
		[ShowInInspector, ReadOnly]
		[ListDrawerSettings(
			ShowIndexLabels = false,
			ShowPaging = false,
			DraggableItems = false,
			NumberOfItemsPerPage = 8
		)]
		[InfoBox("No commands to redo", InfoMessageType.None, VisibleIf = "@RedoStack.Count == 0")]
		private List<string> RedoStack
		{
			get
			{
				if (_commandQueue == null)
					return new List<string> { "Not connected" };

				var names = _commandQueue.UndoneCommandNames;
				if (names == null || names.Count == 0)
					return new List<string>();

				return names.Select((n, i) => i == 0 ? $"▶ {n}" : $"  {n}").ToList();
			}
		}

		#endregion

		#region Control Panel

		[TitleGroup("Control Panel")]
		[HorizontalGroup("Control Panel/Row1")]
		[Button("Pause", ButtonSizes.Medium), GUIColor(1f, 0.7f, 0.3f)]
		[EnableIf("@IsConnected && !IsPaused")]
		private void Pause()
		{
			_commandQueue?.Pause();
			Debug.Log("[CommandQueueDebugger] Paused");
		}

		[HorizontalGroup("Control Panel/Row1")]
		[Button("Resume", ButtonSizes.Medium), GUIColor(0.3f, 1f, 0.5f)]
		[EnableIf("@IsConnected && IsPaused")]
		private void Resume()
		{
			_commandQueue?.Resume();
			Debug.Log("[CommandQueueDebugger] Resumed");
		}

		[HorizontalGroup("Control Panel/Row1")]
		[Button("Execute All", ButtonSizes.Medium), GUIColor(0.3f, 0.8f, 1f)]
		[EnableIf("@IsConnected && PendingCount > 0 && !IsExecuting")]
		private void ExecuteAll()
		{
			_commandQueue?.ExecuteAll();
			Debug.Log("[CommandQueueDebugger] ExecuteAll triggered");
		}

		[HorizontalGroup("Control Panel/Row2")]
		[Button("Undo", ButtonSizes.Medium), GUIColor(0.4f, 0.7f, 1f)]
		[EnableIf("@IsConnected && UndoCount > 0 && !IsExecuting")]
		private void Undo()
		{
			if (_commandQueue?.Undo() == true)
				Debug.Log("[CommandQueueDebugger] Undo performed");
		}

		[HorizontalGroup("Control Panel/Row2")]
		[Button("Redo", ButtonSizes.Medium), GUIColor(0.7f, 0.4f, 1f)]
		[EnableIf("@IsConnected && RedoCount > 0 && !IsExecuting")]
		private void Redo()
		{
			if (_commandQueue?.Redo() == true)
				Debug.Log("[CommandQueueDebugger] Redo performed");
		}

		[HorizontalGroup("Control Panel/Row3")]
		[Button("Clear Pending", ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.3f)]
		[EnableIf("@IsConnected && PendingCount > 0")]
		private void ClearPending()
		{
			_commandQueue?.Clear();
			Debug.Log("[CommandQueueDebugger] Pending commands cleared");
		}

		[HorizontalGroup("Control Panel/Row3")]
		[Button("Clear All", ButtonSizes.Medium), GUIColor(1f, 0.3f, 0.3f)]
		[EnableIf("@IsConnected")]
		private void ClearAll()
		{
			_commandQueue?.ClearAll();
			Debug.Log("[CommandQueueDebugger] All history cleared");
		}

		#endregion

		#region Private Fields

		private CommandQueue _commandQueue;

		#endregion

		#region Unity Lifecycle

		private void OnEnable()
		{
			if (Application.isPlaying)
				TryConnect();
		}

		private void Update()
		{
			// Keep trying to connect until successful
			if (Application.isPlaying && _commandQueue == null)
				TryConnect();
		}

		private void OnDisable() => _commandQueue = null;

		#endregion

		#region Connection

		private void TryConnect()
		{
			// Check if RootContainer is available
			if (!RootContainer.Instance)
				return;

			// Try to resolve CommandQueue (concrete type, not interface)
			var queue = RootContainer.Instance.TryResolve<ICommandQueue>();
			_commandQueue = queue as CommandQueue;
		}

		#endregion
	}
}

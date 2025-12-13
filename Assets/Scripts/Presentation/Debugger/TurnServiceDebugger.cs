using System.Collections.Generic;
using System.Linq;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Turn;
using UnityEngine;

namespace Presentation.Debugger
{
	[AddComponentMenu("Debugger/Turn Service Debugger")]
	public class TurnServiceDebugger : MonoBehaviour
	{
		#region Connection

		[TitleGroup("Connection", order: -100)]
		[ShowInInspector, ReadOnly]
		[GUIColor("@IsConnected ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f)")]
		private bool IsConnected => _turnService != null;

		[TitleGroup("Connection")]
		[ShowInInspector, ReadOnly, DisplayAsString]
		[HideIf("IsConnected")]
		private string ConnectionHint => "Waiting for target...";

		#endregion

		#region Turn Info

		[TitleGroup("Turn Info", boldTitle: true)]
		[HorizontalGroup("Turn Info/Row1")]
		[BoxGroup("Turn Info/Row1/Numbers")]
		[ShowInInspector, ReadOnly]
		[LabelText("Turn"), LabelWidth(80)]
		[GUIColor(0.3f, 0.8f, 1f)]
		private int CurrentTurnNumber => _turnService?.GetCurrentTurnNumber() ?? 0;

		[BoxGroup("Turn Info/Row1/Numbers")]
		[ShowInInspector, ReadOnly]
		[LabelText("Remaining"), LabelWidth(80)]
		[GUIColor("@RemainingUnits > 0 ? new Color(0.3f, 1f, 0.6f) : new Color(0.7f, 0.7f, 0.7f)")]
		private int RemainingUnits => _turnService?.GetRemainingUnitsCount() ?? 0;

		[BoxGroup("Turn Info/Row1/Status")]
		[ShowInInspector, ReadOnly]
		[LabelText("Active"), LabelWidth(80)]
		[GUIColor("@IsTurnActive ? new Color(0.3f, 1f, 0.3f) : new Color(0.7f, 0.7f, 0.7f)")]
		private bool IsTurnActive => _turnService?.IsTurnActive() ?? false;

		[BoxGroup("Turn Info/Row1/Status")]
		[ShowInInspector, ReadOnly]
		[LabelText("Complete"), LabelWidth(80)]
		[GUIColor("@IsTurnComplete ? new Color(1f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f)")]
		private bool IsTurnComplete => _turnService?.IsCurrentTurnComplete() ?? false;

		#endregion

		#region Current Acting Unit

		[TitleGroup("Current Acting Unit", boldTitle: true)]
		[ShowInInspector, ReadOnly]
		[InfoBox("No unit currently acting", InfoMessageType.None, VisibleIf = "@!HasCurrentUnit")]
		[HideIf("@!HasCurrentUnit")]
		[InlineProperty, HideLabel]
		private UnitDisplayInfo CurrentUnitInfo => GetCurrentUnitInfo();

		private bool HasCurrentUnit => _turnService?.HasCurrentUnit() ?? false;

		#endregion

		#region Action Queue

		[TitleGroup("Action Queue", "Units waiting to act (ordered by Speed → Priority)")]
		[ShowInInspector, ReadOnly]
		[TableList(
			AlwaysExpanded = true,
			IsReadOnly = true,
			ShowIndexLabels = true
		)]
		[InfoBox("Queue is empty", InfoMessageType.None, VisibleIf = "@ActionQueue.Count == 0")]
		private List<UnitQueueEntry> ActionQueue
		{
			get
			{
				if (_turnService == null)
					return new List<UnitQueueEntry>();

				var turnOrder = _turnService.GetTurnOrder();
				if (turnOrder == null || turnOrder.Count == 0)
					return new List<UnitQueueEntry>();

				return turnOrder.Select((unit, index) => new UnitQueueEntry
				{
					position = index + 1,
					unitId = unit.Id,
					speed = unit.Speed,
					priority = unit.ActionPriority,
					canAct = unit.CanAct
				}).ToList();
			}
		}

		#endregion

		#region Control Panel

		[TitleGroup("Control Panel")]
		[HorizontalGroup("Control Panel/Row1")]
		[Button("Start Turn", ButtonSizes.Large), GUIColor(0.3f, 1f, 0.5f)]
		[EnableIf("@IsConnected && !IsTurnActive")]
		private void StartTurn()
		{
			_turnService?.StartTurn();
			Debug.Log("[TurnServiceDebugger] Turn started");
		}

		[HorizontalGroup("Control Panel/Row1")]
		[Button("End Turn", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.3f)]
		[EnableIf("@IsConnected && IsTurnActive")]
		private void EndTurn()
		{
			_turnService?.EndTurn();
			Debug.Log("[TurnServiceDebugger] Turn ended");
		}

		[HorizontalGroup("Control Panel/Row2")]
		[Button("Next Unit", ButtonSizes.Medium), GUIColor(0.3f, 0.8f, 1f)]
		[EnableIf("@IsConnected && IsTurnActive && !HasCurrentUnit && RemainingUnits > 0")]
		private void NextUnit()
		{
			var unit = _turnService?.NextUnit();
			Debug.Log($"[TurnServiceDebugger] Next unit: {unit?.Id ?? "None"}");
		}

		[HorizontalGroup("Control Panel/Row2")]
		[Button("End Unit Turn", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.3f)]
		[EnableIf("@IsConnected && HasCurrentUnit")]
		private void EndUnitTurn()
		{
			_turnService?.EndUnitTurn();
			Debug.Log("[TurnServiceDebugger] Unit turn ended");
		}

		[HorizontalGroup("Control Panel/Row3")]
		[Button("Clear All", ButtonSizes.Medium), GUIColor(1f, 0.3f, 0.3f)]
		[EnableIf("IsConnected")]
		private void Clear()
		{
			_turnService?.Clear();
			Debug.Log("[TurnServiceDebugger] Turn service cleared");
		}

		#endregion

		#region Advanced Operations

		[TitleGroup("Advanced Operations", "Unit manipulation")]
		[HorizontalGroup("Advanced Operations/Row1")]
		[SerializeField]
		[LabelText("Unit ID"), LabelWidth(60)]
		private string targetUnitId = "";

		[HorizontalGroup("Advanced Operations/Row1")]
		[SerializeField]
		[LabelText("Priority"), LabelWidth(50)]
		private int insertPriority = 999;

		[HorizontalGroup("Advanced Operations/Row2")]
		[Button("Force Insert", ButtonSizes.Medium), GUIColor(0.8f, 0.5f, 1f)]
		[EnableIf("@IsConnected && !string.IsNullOrEmpty(targetUnitId)")]
		private void ForceInsertUnit()
		{
			if (string.IsNullOrEmpty(targetUnitId)) return;
			_turnService?.ForceInsertUnit(targetUnitId, insertPriority);
			Debug.Log($"[TurnServiceDebugger] Force inserted unit '{targetUnitId}' with priority {insertPriority}");
		}

		[HorizontalGroup("Advanced Operations/Row2")]
		[Button("Update Speed", ButtonSizes.Medium), GUIColor(0.5f, 0.8f, 1f)]
		[EnableIf("@IsConnected && !string.IsNullOrEmpty(targetUnitId)")]
		private void UpdateUnitSpeed()
		{
			if (string.IsNullOrEmpty(targetUnitId)) return;
			_turnService?.UpdateUnitSpeed(targetUnitId);
			Debug.Log($"[TurnServiceDebugger] Updated speed for unit '{targetUnitId}'");
		}

		[HorizontalGroup("Advanced Operations/Row2")]
		[Button("Unregister", ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
		[EnableIf("@IsConnected && !string.IsNullOrEmpty(targetUnitId)")]
		private void UnregisterUnit()
		{
			if (string.IsNullOrEmpty(targetUnitId)) return;
			_turnService?.UnregisterUnit(targetUnitId);
			Debug.Log($"[TurnServiceDebugger] Unregistered unit '{targetUnitId}'");
		}

		#endregion

		#region Private Fields

		private ITurnService _turnService;

		#endregion

		#region Unity Lifecycle

		private void OnEnable()
		{
			if (Application.isPlaying)
				TryConnect();
		}

		private void Update()
		{
			if (Application.isPlaying && _turnService == null)
				TryConnect();
		}

		private void OnDisable() => _turnService = null;

		#endregion

		#region Connection

		private void TryConnect()
		{
			// TurnService is registered in LevelContainer
			if (!LevelContainer.Instance)
				return;

			_turnService = LevelContainer.Instance.TryResolve<ITurnService>();
		}

		#endregion

		#region Helper Methods

		private UnitDisplayInfo GetCurrentUnitInfo()
		{
			var unit = _turnService?.GetCurrentUnit();
			if (unit == null)
				return new UnitDisplayInfo { unitId = "None" };

			return new UnitDisplayInfo
			{
				unitId = unit.Id,
				speed = unit.Speed,
				priority = unit.ActionPriority,
				canAct = unit.CanAct,
				queuePosition = _turnService?.GetUnitPositionInQueue(unit.Id) ?? -1
			};
		}

		#endregion

		#region Display Data Structures

		/// <summary>
		/// Display structure for the current acting unit.
		/// </summary>
		[System.Serializable]
		private struct UnitDisplayInfo
		{
			[HorizontalGroup("Info"), LabelWidth(50)]
			[GUIColor(0.3f, 1f, 0.6f)]
			public string unitId;

			[HorizontalGroup("Info"), LabelWidth(45)]
			public int speed;

			[HorizontalGroup("Info"), LabelWidth(50)]
			public int priority;

			[HorizontalGroup("Info"), LabelWidth(50)]
			public bool canAct;

			[HorizontalGroup("Info"), LabelWidth(80)]
			[LabelText("Queue Pos")]
			public int queuePosition;
		}

		/// <summary>
		/// Display structure for units in the action queue.
		/// </summary>
		[System.Serializable]
		private struct UnitQueueEntry
		{
			[TableColumnWidth(40, Resizable = false)]
			[LabelText("#")]
			public int position;

			[TableColumnWidth(120)]
			[LabelText("Unit ID")]
			public string unitId;

			[TableColumnWidth(60, Resizable = false)]
			public int speed;

			[TableColumnWidth(60, Resizable = false)]
			public int priority;

			[TableColumnWidth(60, Resizable = false)]
			[LabelText("Can Act")]
			public bool canAct;
		}

		#endregion
	}
}

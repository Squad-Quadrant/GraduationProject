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
		private int TurnNumber => _turnService?.TurnNumber ?? 0;

		[BoxGroup("Turn Info/Row1/Numbers")]
		[ShowInInspector, ReadOnly]
		[LabelText("Upcoming"), LabelWidth(80)]
		[GUIColor("@UpcomingCount > 0 ? new Color(0.3f, 1f, 0.6f) : new Color(0.7f, 0.7f, 0.7f)")]
		private int UpcomingCount => _turnService?.GetUpcoming().Count ?? 0;

		[BoxGroup("Turn Info/Row1/Status")]
		[ShowInInspector, ReadOnly]
		[LabelText("Turn Active"), LabelWidth(80)]
		[GUIColor("@IsTurnActive ? new Color(0.3f, 1f, 0.3f) : new Color(0.7f, 0.7f, 0.7f)")]
		private bool IsTurnActive => _turnService?.IsTurnActive ?? false;

		[BoxGroup("Turn Info/Row1/Status")]
		[ShowInInspector, ReadOnly]
		[LabelText("Unit Acting"), LabelWidth(80)]
		[GUIColor("@IsUnitActing ? new Color(1f, 0.9f, 0.3f) : new Color(0.7f, 0.7f, 0.7f)")]
		private bool IsUnitActing => _turnService?.IsUnitActing ?? false;

		[BoxGroup("Turn Info/Row1/Status")]
		[ShowInInspector, ReadOnly]
		[LabelText("Complete"), LabelWidth(80)]
		[GUIColor("@IsTurnComplete ? new Color(1f, 0.8f, 0.3f) : new Color(0.7f, 0.7f, 0.7f)")]
		private bool IsTurnComplete => _turnService?.IsTurnComplete ?? false;

		#endregion

		#region Active Unit

		[TitleGroup("Active Unit", boldTitle: true)]
		[ShowInInspector, ReadOnly]
		[InfoBox("No unit currently acting", InfoMessageType.None, VisibleIf = "@!IsUnitActing")]
		[HideIf("@!IsUnitActing")]
		[InlineProperty, HideLabel]
		private UnitDisplayInfo ActiveUnitInfo => GetActiveUnitInfo();

		#endregion

		#region Full Turn Order

		[TitleGroup("Turn Order", "Complete queue: acted → current → upcoming")]
		[ShowInInspector, ReadOnly]
		[TableList(AlwaysExpanded = true, IsReadOnly = true, ShowIndexLabels = false)]
		[InfoBox("Queue is empty", InfoMessageType.None, VisibleIf = "@FullOrder.Count == 0")]
		private List<QueueEntry> FullOrder
		{
			get
			{
				if (_turnService == null) return new List<QueueEntry>();

				var fullOrder = _turnService.GetFullOrder();
				if (fullOrder == null || fullOrder.Count == 0) return new List<QueueEntry>();

				var activeUnit = _turnService.ActiveUnit;

				return fullOrder.Select((unit, index) =>
				{
					// Determine segment based on relationship to active unit
					var activeIndex = activeUnit != null
						? _turnService.IndexOf(activeUnit.Id)
						: -1;

					EQueueSegment segment;
					if (activeIndex < 0)
						segment = EQueueSegment.Upcoming; // No active unit, all are upcoming
					else if (index < activeIndex)
						segment = EQueueSegment.Acted;
					else if (index == activeIndex)
						segment = EQueueSegment.Current;
					else
						segment = EQueueSegment.Upcoming;

					return new QueueEntry
					{
						index = index,
						segment = segment,
						unitId = unit.Id,
						speed = unit.Speed,
						priority = unit.ActionPriority,
						canAct = unit.CanAct
					};
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
		[EnableIf("@IsConnected && IsTurnActive && !IsUnitActing")]
		private void NextUnit()
		{
			var unit = _turnService?.NextUnit();
			Debug.Log($"[TurnServiceDebugger] Next unit: {unit?.Id ?? "None"}");
		}

		[HorizontalGroup("Control Panel/Row2")]
		[Button("End Unit Turn", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.3f)]
		[EnableIf("@IsConnected && IsUnitActing")]
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
			Debug.Log("[TurnServiceDebugger] Cleared");
		}

		#endregion

		#region Queue Manipulation

		[TitleGroup("Queue Manipulation", "Modify turn order mid-turn")]
		[HorizontalGroup("Queue Manipulation/Row1")]
		[SerializeField, LabelText("Unit ID"), LabelWidth(60)]
		private string targetUnitId = "";

		[HorizontalGroup("Queue Manipulation/Row1")]
		[SerializeField, LabelText("Priority"), LabelWidth(50)]
		private int targetPriority = 0;

		[HorizontalGroup("Queue Manipulation/Row2")]
		[Button("Set Priority", ButtonSizes.Medium), GUIColor(0.8f, 0.5f, 1f)]
		[EnableIf("@IsConnected && IsTurnActive && !string.IsNullOrEmpty(targetUnitId)")]
		private void SetPriority()
		{
			if (string.IsNullOrEmpty(targetUnitId)) return;
			_turnService?.SetUnitPriority(targetUnitId, targetPriority);
			Debug.Log($"[TurnServiceDebugger] Set priority of '{targetUnitId}' to {targetPriority}");
		}

		[HorizontalGroup("Queue Manipulation/Row2")]
		[Button("Move To Next", ButtonSizes.Medium), GUIColor(0.5f, 0.8f, 1f)]
		[EnableIf("@IsConnected && IsTurnActive && !string.IsNullOrEmpty(targetUnitId)")]
		private void MoveToNext()
		{
			if (string.IsNullOrEmpty(targetUnitId)) return;
			_turnService?.MoveUnitToNext(targetUnitId);
			Debug.Log($"[TurnServiceDebugger] Moved '{targetUnitId}' to next");
		}

		[HorizontalGroup("Queue Manipulation/Row2")]
		[Button("Resort Queue", ButtonSizes.Medium), GUIColor(0.6f, 0.8f, 0.6f)]
		[EnableIf("@IsConnected && IsTurnActive")]
		private void ResortQueue()
		{
			_turnService?.ResortQueue();
			Debug.Log("[TurnServiceDebugger] Queue re-sorted");
		}

		[HorizontalGroup("Queue Manipulation/Row3")]
		[Button("Remove Unit", ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
		[EnableIf("@IsConnected && !string.IsNullOrEmpty(targetUnitId)")]
		private void RemoveUnit()
		{
			if (string.IsNullOrEmpty(targetUnitId)) return;
			_turnService?.RemoveUnit(targetUnitId);
			Debug.Log($"[TurnServiceDebugger] Removed '{targetUnitId}'");
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
			if (!LevelContainer.Instance) return;
			_turnService = LevelContainer.Instance.TryResolve<ITurnService>();
		}

		#endregion

		#region Helper Methods

		private UnitDisplayInfo GetActiveUnitInfo()
		{
			var unit = _turnService?.ActiveUnit;
			if (unit == null)
				return new UnitDisplayInfo { unitId = "None" };

			return new UnitDisplayInfo
			{
				unitId = unit.Id,
				speed = unit.Speed,
				priority = unit.ActionPriority,
				canAct = unit.CanAct,
				queuePosition = _turnService?.IndexOf(unit.Id) ?? -1
			};
		}

		#endregion

		#region Display Data Structures

		private enum EQueueSegment { Acted, Current, Upcoming }

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

		[System.Serializable]
		private struct QueueEntry
		{
			[TableColumnWidth(30, Resizable = false)]
			[LabelText("#")]
			public int index;

			[TableColumnWidth(70, Resizable = false)]
			[GUIColor("@segment == EQueueSegment.Current ? new Color(1f, 0.9f, 0.3f) " +
			          ": segment == EQueueSegment.Acted ? new Color(0.5f, 0.5f, 0.5f) " +
			          ": new Color(0.7f, 1f, 0.7f)")]
			public EQueueSegment segment;

			[TableColumnWidth(120)]
			[LabelText("Unit ID")]
			public string unitId;

			[TableColumnWidth(50, Resizable = false)]
			public int speed;

			[TableColumnWidth(50, Resizable = false)]
			[LabelText("P")]
			public int priority;

			[TableColumnWidth(50, Resizable = false)]
			[LabelText("Act")]
			public bool canAct;
		}

		#endregion
	}
}

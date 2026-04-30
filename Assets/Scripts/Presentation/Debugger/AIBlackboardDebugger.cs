using System;
using System.Collections.Generic;
using System.Linq;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.AI.Blackboard;
using Systems.Turn;
using Systems.Unit;
using UnityEngine;

namespace Presentation.Debugger
{
	[AddComponentMenu("Debugger/AI Blackboard Debugger")]
	public class AIBlackboardDebugger : MonoBehaviour
	{
		#region Connection

		[TitleGroup("Connection", order: -100)]
		[ShowInInspector, ReadOnly]
		[GUIColor("@IsConnected ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f)")]
		private bool IsConnected => _blackboardService != null;

		[TitleGroup("Connection")]
		[ShowInInspector, ReadOnly, DisplayAsString]
		[HideIf("IsConnected")]
		private string ConnectionHint => "Waiting for target...";

		#endregion

		#region Faction Selector

		[TitleGroup("Faction", boldTitle: true)]
		[ShowInInspector]
		[LabelText("View Faction"), LabelWidth(100)]
		[ValueDropdown(nameof(GetSelectableFactions))]
		[EnableIf("IsConnected")]
		private EUnitFaction _selectedFaction = EUnitFaction.Enemy;

		private static IEnumerable<EUnitFaction> GetSelectableFactions() => new[]
		{
			EUnitFaction.Enemy,
			EUnitFaction.Neutral,
		};

		[TitleGroup("Faction")]
		[ShowInInspector, ReadOnly]
		[LabelText("Has Blackboard"), LabelWidth(100)]
		[GUIColor("@HasBoard ? new Color(0.3f, 0.8f, 1f) : new Color(0.7f, 0.7f, 0.7f)")]
		private bool HasBoard => CurrentBoard != null;

		private AIBlackboard CurrentBoard => IsConnected
			? _blackboardService.GetBlackboard(_selectedFaction)
			: null;

		#endregion

		#region Known Enemies

		[TitleGroup("Known Enemies", "Enemies that have ever been seen by any member of the selected faction")]
		[ShowInInspector, ReadOnly]
		[LabelText("Count"), LabelWidth(80)]
		[GUIColor("@KnownEnemyCount > 0 ? new Color(1f, 0.6f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
		private int KnownEnemyCount => CurrentBoard?.KnownEnemies.Count ?? 0;

		[TitleGroup("Known Enemies")]
		[ShowInInspector, ReadOnly]
		[TableList(AlwaysExpanded = true, IsReadOnly = true, ShowIndexLabels = false, NumberOfItemsPerPage = 10)]
		[InfoBox("No known enemies", InfoMessageType.None, VisibleIf = "@KnownEnemyList.Count == 0")]
		private List<KnownEnemyEntry> KnownEnemyList
		{
			get
			{
				var board = CurrentBoard;
				if (board == null) return new List<KnownEnemyEntry>();

				int now = _turnService?.TurnNumber ?? 0;
				return board.KnownEnemies.Values
					.OrderByDescending(k => k.LastSeenTurn)
					.Select(k => new KnownEnemyEntry
					{
						EnemyId = k.EnemyUnitId ?? "null",
						LastKnownPos = k.LastKnownPos,
						LastSeenTurn = k.LastSeenTurn,
						AgeTurns = now - k.LastSeenTurn,
						LastSeenBy = k.LastSeenByMemberId ?? "null",
					})
					.ToList();
			}
		}

		#endregion

		#region Recent Threats

		[TitleGroup("Recent Threats", "Damage incidents recorded within the expiration window")]
		[ShowInInspector, ReadOnly]
		[LabelText("Count"), LabelWidth(80)]
		[GUIColor("@ThreatCount > 0 ? new Color(1f, 0.4f, 0.4f) : new Color(0.5f, 0.5f, 0.5f)")]
		private int ThreatCount => CurrentBoard?.RecentThreats.Count ?? 0;

		[TitleGroup("Recent Threats")]
		[ShowInInspector, ReadOnly]
		[TableList(AlwaysExpanded = true, IsReadOnly = true, ShowIndexLabels = false, NumberOfItemsPerPage = 10)]
		[InfoBox("No recent threats", InfoMessageType.None, VisibleIf = "@ThreatList.Count == 0")]
		private List<ThreatEntry> ThreatList
		{
			get
			{
				var board = CurrentBoard;
				if (board == null) return new List<ThreatEntry>();

				int now = _turnService?.TurnNumber ?? 0;
				// 倒序：最新的威胁排最上面
				return board.RecentThreats
					.Reverse()
					.Select(t => new ThreatEntry
					{
						DamagedId = t.DamagedUnitId ?? "null",
						DamagedAtPos = t.DamagedAtPos,
						Turn = t.Turn,
						AgeTurns = now - t.Turn,
						AttackerId = t.AttackerUnitId ?? "<non-unit>",
						AttackerPos = t.AttackerPos.HasValue ? t.AttackerPos.Value.ToString() : "-",
					})
					.ToList();
			}
		}

		#endregion

		#region Display Data Structures

		[Serializable]
		private struct KnownEnemyEntry
		{
			[TableColumnWidth(140)]
			[LabelText("Enemy Id")]
			public string EnemyId;

			[TableColumnWidth(90, Resizable = false)]
			[LabelText("Last Pos")]
			public Vector2Int LastKnownPos;

			[TableColumnWidth(70, Resizable = false)]
			[LabelText("Seen T")]
			public int LastSeenTurn;

			[TableColumnWidth(60, Resizable = false)]
			[LabelText("Age")]
			[GUIColor("@AgeTurns <= 0 ? new Color(0.3f, 1f, 0.6f) : (AgeTurns <= 2 ? new Color(1f, 0.9f, 0.3f) : new Color(0.7f, 0.7f, 0.7f))")]
			public int AgeTurns;

			[TableColumnWidth(140)]
			[LabelText("Reporter")]
			public string LastSeenBy;
		}

		[Serializable]
		private struct ThreatEntry
		{
			[TableColumnWidth(140)]
			[LabelText("Damaged Id")]
			public string DamagedId;

			[TableColumnWidth(90, Resizable = false)]
			[LabelText("At Pos")]
			public Vector2Int DamagedAtPos;

			[TableColumnWidth(60, Resizable = false)]
			[LabelText("Turn")]
			public int Turn;

			[TableColumnWidth(60, Resizable = false)]
			[LabelText("Age")]
			[GUIColor("@AgeTurns <= 0 ? new Color(1f, 0.4f, 0.4f) : (AgeTurns <= 2 ? new Color(1f, 0.9f, 0.3f) : new Color(0.7f, 0.7f, 0.7f))")]
			public int AgeTurns;

			[TableColumnWidth(140)]
			[LabelText("Attacker Id")]
			public string AttackerId;

			[TableColumnWidth(90, Resizable = false)]
			[LabelText("Attacker Pos")]
			public string AttackerPos;
		}

		#endregion

		#region Connection Lifecycle

		private IAIBlackboardService _blackboardService;
		private ITurnService _turnService;

		private void OnEnable()
		{
			if (Application.isPlaying)
				TryConnect();
		}

		private void Update()
		{
			if (Application.isPlaying && _blackboardService == null)
				TryConnect();
		}

		private void OnDisable()
		{
			_blackboardService = null;
			_turnService = null;
		}

		private void TryConnect()
		{
			if (!LevelContainer.Instance) return;

			_blackboardService = LevelContainer.Instance.TryResolve<IAIBlackboardService>();
			_turnService = LevelContainer.Instance.TryResolve<ITurnService>();
		}

		#endregion
	}
}

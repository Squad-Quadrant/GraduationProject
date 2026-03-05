using System;
using System.Collections.Generic;
using System.Linq;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Unit;
using UnityEngine;

namespace Presentation.Debugger
{
	/// <summary>
	/// Debugger for UnitService.
	/// Auto-connects to UnitService from LevelContainer.
	/// Displays all units, their stats, and provides inspection/manipulation tools.
	/// </summary>
	[AddComponentMenu("Debugger/Unit Service Debugger")]
	public class UnitServiceDebugger : MonoBehaviour
	{
		#region Connection

		[TitleGroup("Connection", order: -100)]
		[ShowInInspector, ReadOnly]
		[GUIColor("@IsConnected ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f)")]
		private bool IsConnected => _unitService != null;

		[TitleGroup("Connection")]
		[ShowInInspector, ReadOnly, DisplayAsString]
		[HideIf("IsConnected")]
		private string ConnectionHint => "Waiting for Target...";

		#endregion

		#region Overview

		[TitleGroup("Overview", boldTitle: true)]
		[HorizontalGroup("Overview/Stats")]
		[BoxGroup("Overview/Stats/Counts")]
		[ShowInInspector, ReadOnly]
		[LabelText("Total"), LabelWidth(60)]
		[GUIColor(0.3f, 0.8f, 1f)]
		private int TotalCount => _unitService?.Count ?? 0;

		[BoxGroup("Overview/Stats/Counts")]
		[ShowInInspector, ReadOnly]
		[LabelText("Alive"), LabelWidth(60)]
		[GUIColor("@AliveCount > 0 ? new Color(0.3f, 1f, 0.6f) : new Color(1f, 0.4f, 0.4f)")]
		private int AliveCount => _unitService?.GetAllAliveUnits()?.Count ?? 0;

		[BoxGroup("Overview/Stats/Counts")]
		[ShowInInspector, ReadOnly]
		[LabelText("Dead"), LabelWidth(60)]
		[GUIColor("@DeadCount > 0 ? new Color(1f, 0.5f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
		private int DeadCount => TotalCount - AliveCount;

		#endregion

		#region Unit List

		[TitleGroup("Unit List", "All registered units")]
		[ShowInInspector, ReadOnly]
		[TableList(AlwaysExpanded = true, IsReadOnly = true, ShowIndexLabels = false, NumberOfItemsPerPage = 12)]
		[InfoBox("No units registered", InfoMessageType.None, VisibleIf = "@UnitList.Count == 0")]
		private List<UnitListEntry> UnitList
		{
			get
			{
				if (_unitService == null)
					return new List<UnitListEntry>();

				var units = _unitService.GetAllUnits();
				if (units == null || units.Count == 0)
					return new List<UnitListEntry>();

				// All fields now accessed directly on unit — no stats/runtime indirection
				return units.Select(u => new UnitListEntry
				{
					Id = u.id ?? "null",
					Name = u.name ?? "null",
					Position = u.position,
					CurrentHp = u.currentHp,
					MaxHp = u.maxHp,
					Speed = u.speed,
					IsAlive = u.IsAlive,
					IsStunned = u.isStunned
				}).ToList();
			}
		}

		#endregion

		#region Unit Selection

		[TitleGroup("Unit Inspector", "Select a unit to view details")]
		[HorizontalGroup("Unit Inspector/Selection")]
		[ValueDropdown("GetUnitIdOptions")]
		[LabelText("Select Unit"), LabelWidth(80)]
		[SerializeField]
		private string _selectedUnitId = "";

		private IEnumerable<string> GetUnitIdOptions()
		{
			var options = new List<string> { "" };
			if (_unitService == null) return options;

			var units = _unitService.GetAllUnits();
			if (units != null)
				options.AddRange(units.Select(u => u.id));
			return options;
		}

		#endregion

		#region Selected Unit Details

		[TitleGroup("Selected Unit Details")]
		[ShowInInspector, ReadOnly]
		[InfoBox("Select a unit above to view details", InfoMessageType.None, VisibleIf = "@!HasSelectedUnit")]
		[ShowIf("HasSelectedUnit")]
		[InlineProperty, HideLabel]
		private UnitDetailInfo SelectedUnitDetails => GetSelectedUnitDetails();

		private bool HasSelectedUnit
		{
			get
			{
				if (string.IsNullOrEmpty(_selectedUnitId)) return false;
				if (_unitService == null) return false;
				return _unitService.HasUnit(_selectedUnitId);
			}
		}

		#endregion

		#region Selected Unit State (merged stats + runtime)

		[TitleGroup("Selected Unit State")]
		[ShowIf("HasSelectedUnit")]
		[ShowInInspector, ReadOnly]
		[InlineProperty, HideLabel]
		private UnitStateInfo SelectedUnitState => GetSelectedUnitState();

		#endregion

		#region Control Panel

		[TitleGroup("Control Panel")]
		[HorizontalGroup("Control Panel/Row1")]
		[Button("Destroy Selected", ButtonSizes.Large), GUIColor(1f, 0.4f, 0.4f)]
		[EnableIf("@IsConnected && HasSelectedUnit")]
		private void DestroySelectedUnit()
		{
			if (_unitService == null || string.IsNullOrEmpty(_selectedUnitId))
				return;

			var unit = _unitService.GetUnit(_selectedUnitId);
			var unitName = unit?.name ?? _selectedUnitId;
			_unitService.DestroyUnit(_selectedUnitId);
			Debug.Log($"[UnitServiceDebugger] Destroyed unit: {unitName}");
			_selectedUnitId = "";
		}

		[HorizontalGroup("Control Panel/Row1")]
		[Button("Clear All Units", ButtonSizes.Large), GUIColor(1f, 0.3f, 0.3f)]
		[EnableIf("@IsConnected && TotalCount > 0")]
		private void ClearAllUnits()
		{
			_unitService?.Clear();
			_selectedUnitId = "";
			Debug.Log("[UnitServiceDebugger] All units cleared");
		}

		#endregion

		#region Debug Actions

		[TitleGroup("Debug Actions", "Modify selected unit (for testing)")]
		[HorizontalGroup("Debug Actions/HP")]
		[SerializeField]
		[LabelText("HP Change"), LabelWidth(80)]
		[PropertyRange(-100, 100)]
		private int _hpChange = -10;

		[HorizontalGroup("Debug Actions/HP", Width = 120)]
		[Button("Apply HP"), GUIColor(0.8f, 0.5f, 0.5f)]
		[EnableIf("HasSelectedUnit")]
		private void ApplyHpChange()
		{
			var unit = GetSelectedUnit();
			if (unit == null) return;

			var oldHp = unit.currentHp;
			unit.currentHp = Mathf.Clamp(unit.currentHp + _hpChange, 0, unit.maxHp);
			Debug.Log($"[UnitServiceDebugger] {unit.name} HP: {oldHp} -> {unit.currentHp}");
		}

		[HorizontalGroup("Debug Actions/Status")]
		[Button("Toggle Stun", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.3f)]
		[EnableIf("HasSelectedUnit")]
		private void ToggleStun()
		{
			var unit = GetSelectedUnit();
			if (unit == null) return;

			unit.isStunned = !unit.isStunned;
			Debug.Log($"[UnitServiceDebugger] {unit.name} Stunned: {unit.isStunned}");
		}

		[HorizontalGroup("Debug Actions/Status")]
		[Button("Kill Unit", ButtonSizes.Medium), GUIColor(1f, 0.3f, 0.3f)]
		[EnableIf("@HasSelectedUnit && SelectedUnitIsAlive")]
		private void KillUnit()
		{
			var unit = GetSelectedUnit();
			if (unit == null) return;

			unit.currentHp = 0;
			Debug.Log($"[UnitServiceDebugger] {unit.name} killed (HP set to 0)");
		}

		[HorizontalGroup("Debug Actions/Status")]
		[Button("Revive Unit", ButtonSizes.Medium), GUIColor(0.3f, 1f, 0.5f)]
		[EnableIf("@HasSelectedUnit && !SelectedUnitIsAlive")]
		private void ReviveUnit()
		{
			var unit = GetSelectedUnit();
			if (unit == null) return;

			unit.currentHp = unit.maxHp;
			Debug.Log($"[UnitServiceDebugger] {unit.name} revived (HP set to max)");
		}

		private bool SelectedUnitIsAlive
		{
			get
			{
				var unit = GetSelectedUnit();
				return unit?.IsAlive ?? false;
			}
		}

		#endregion

		#region Private Fields

		private IUnitService _unitService;

		#endregion

		#region Unity Lifecycle

		private void OnEnable()
		{
			if (Application.isPlaying)
				TryConnect();
		}

		private void Update()
		{
			if (Application.isPlaying && _unitService == null)
				TryConnect();
		}

		private void OnDisable() => _unitService = null;

		#endregion

		#region Connection

		private void TryConnect()
		{
			if (LevelContainer.Instance == null) return;
			_unitService = LevelContainer.Instance.TryResolve<IUnitService>();
		}

		#endregion

		#region Helper Methods

		private Systems.Unit.Unit GetSelectedUnit()
		{
			if (_unitService == null || string.IsNullOrEmpty(_selectedUnitId))
				return null;
			_unitService.TryGetUnit(_selectedUnitId, out var unit);
			return unit;
		}

		private UnitDetailInfo GetSelectedUnitDetails()
		{
			var unit = GetSelectedUnit();
			if (unit == null)
				return new UnitDetailInfo { Id = "N/A", ConfigId = "N/A", Name = "N/A" };

			return new UnitDetailInfo
			{
				Id = unit.id ?? "null",
				ConfigId = unit.configId ?? "null",
				Name = unit.name ?? "null",
				Position = unit.position
			};
		}

		/// <summary>
		/// Merged view of what was previously split across UnitStatsInfo + UnitRuntimeInfo.
		/// All data comes from a single Unit object now.
		/// </summary>
		private UnitStateInfo GetSelectedUnitState()
		{
			var unit = GetSelectedUnit();
			if (unit == null) return default;

			var hpPercent = unit.maxHp > 0 ? (float)unit.currentHp / unit.maxHp : 0f;
			return new UnitStateInfo
			{
				CurrentHp = unit.currentHp,
				MaxHp = unit.maxHp,
				HpPercent = hpPercent,
				Speed = unit.speed,
				MoveRange = unit.moveRange,
				ActionPoints = unit.maxAp,
				IsAlive = unit.IsAlive,
				IsStunned = unit.isStunned
			};
		}

		#endregion

		#region Display Data Structures

		[Serializable]
		private struct UnitListEntry
		{
			[TableColumnWidth(100)]
			[LabelText("ID")]
			public string Id;

			[TableColumnWidth(100)]
			public string Name;

			[TableColumnWidth(80)]
			[LabelText("Pos")]
			public Vector2Int Position;

			[TableColumnWidth(70, Resizable = false)]
			[LabelText("HP")]
			[GUIColor("@GetHpColor()")]
			public int CurrentHp;

			[TableColumnWidth(50, Resizable = false)]
			[LabelText("Max")]
			public int MaxHp;

			[TableColumnWidth(50, Resizable = false)]
			[LabelText("Spd")]
			public int Speed;

			[TableColumnWidth(50, Resizable = false)]
			[GUIColor("@IsAlive ? new Color(0.3f, 1f, 0.6f) : new Color(1f, 0.4f, 0.4f)")]
			public bool IsAlive;

			[TableColumnWidth(60, Resizable = false)]
			[LabelText("Stun")]
			[GUIColor("@IsStunned ? new Color(1f, 0.8f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
			public bool IsStunned;

			private Color GetHpColor()
			{
				if (MaxHp <= 0) return new Color(0.5f, 0.5f, 0.5f);
				float ratio = (float)CurrentHp / MaxHp;
				if (ratio > 0.6f) return new Color(0.3f, 1f, 0.6f);
				if (ratio > 0.3f) return new Color(1f, 0.8f, 0.3f);
				return new Color(1f, 0.4f, 0.4f);
			}
		}

		[Serializable]
		private struct UnitDetailInfo
		{
			[HorizontalGroup("Row1")]
			[LabelText("ID"), LabelWidth(60)]
			[GUIColor(0.3f, 0.8f, 1f)]
			public string Id;

			[HorizontalGroup("Row1")]
			[LabelText("Config"), LabelWidth(50)]
			public string ConfigId;

			[HorizontalGroup("Row2")]
			[LabelText("Name"), LabelWidth(60)]
			[GUIColor(0.3f, 1f, 0.6f)]
			public string Name;

			[HorizontalGroup("Row2")]
			[LabelText("Position"), LabelWidth(60)]
			public Vector2Int Position;
		}

		/// <summary>
		/// Unified display struct — replaces the old UnitStatsInfo + UnitRuntimeInfo pair.
		/// </summary>
		[Serializable]
		private struct UnitStateInfo
		{
			[HorizontalGroup("HP")]
			[LabelText("Current HP"), LabelWidth(80)]
			[GUIColor("@GetHpColor()")]
			public int CurrentHp;

			[HorizontalGroup("HP")]
			[LabelText("/"), LabelWidth(10)]
			public int MaxHp;

			[HorizontalGroup("HP")]
			[LabelText(""), LabelWidth(0)]
			[ProgressBar(0, 1, ColorGetter = "GetHpBarColor")]
			public float HpPercent;

			[HorizontalGroup("Stats")]
			[LabelText("Speed"), LabelWidth(50)]
			public int Speed;

			[HorizontalGroup("Stats")]
			[LabelText("Move"), LabelWidth(50)]
			public int MoveRange;

			[HorizontalGroup("Stats")]
			[LabelText("AP"), LabelWidth(30)]
			public int ActionPoints;

			[HorizontalGroup("Status")]
			[LabelText("Alive"), LabelWidth(50)]
			[GUIColor("@IsAlive ? new Color(0.3f, 1f, 0.6f) : new Color(1f, 0.4f, 0.4f)")]
			public bool IsAlive;

			[HorizontalGroup("Status")]
			[LabelText("Stunned"), LabelWidth(60)]
			[GUIColor("@IsStunned ? new Color(1f, 0.8f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
			public bool IsStunned;

			private Color GetHpColor()
			{
				if (HpPercent > 0.6f) return new Color(0.3f, 1f, 0.6f);
				if (HpPercent > 0.3f) return new Color(1f, 0.8f, 0.3f);
				return new Color(1f, 0.4f, 0.4f);
			}

			private Color GetHpBarColor()
			{
				if (HpPercent > 0.6f) return new Color(0.3f, 0.8f, 0.4f);
				if (HpPercent > 0.3f) return new Color(0.9f, 0.7f, 0.2f);
				return new Color(0.9f, 0.3f, 0.3f);
			}
		}

		#endregion
	}
}

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
		[TableList(
			AlwaysExpanded = true,
			IsReadOnly = true,
			ShowIndexLabels = false,
			NumberOfItemsPerPage = 12
		)]
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

				return units.Select(u => new UnitListEntry
				{
					Id = u.id ?? "null",
					Name = u.name ?? "null",
					Position = u.position,
					CurrentHp = u.runtime?.currentHp ?? 0,
					MaxHp = u.stats?.maxHp ?? 0,
					Speed = u.stats?.speed ?? 0,
					IsAlive = u.runtime?.StillAlive ?? false,
					IsStunned = u.runtime?.isStunned ?? false
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

			if (_unitService == null)
				return options;

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
				if (string.IsNullOrEmpty(_selectedUnitId))
					return false;
				if (_unitService == null)
					return false;
				return _unitService.HasUnit(_selectedUnitId);
			}
		}

		#endregion

		#region Unit Stats (Selected)

		[TitleGroup("Selected Unit Stats")]
		[ShowIf("HasSelectedUnit")]
		[ShowInInspector, ReadOnly]
		[InlineProperty, HideLabel]
		private UnitStatsInfo SelectedUnitStats => GetSelectedUnitStats();

		#endregion

		#region Unit Runtime (Selected)

		[TitleGroup("Selected Unit Runtime")]
		[ShowIf("HasSelectedUnit")]
		[ShowInInspector, ReadOnly]
		[InlineProperty, HideLabel]
		private UnitRuntimeInfo SelectedUnitRuntime => GetSelectedUnitRuntime();

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
			if (unit?.runtime == null || unit.stats == null)
				return;

			var oldHp = unit.runtime.currentHp;
			unit.runtime.currentHp = Mathf.Clamp(
				unit.runtime.currentHp + _hpChange,
				0,
				unit.stats.maxHp
			);
			Debug.Log($"[UnitServiceDebugger] {unit.name} HP: {oldHp} -> {unit.runtime.currentHp}");
		}

		[HorizontalGroup("Debug Actions/Status")]
		[Button("Toggle Stun", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.3f)]
		[EnableIf("HasSelectedUnit")]
		private void ToggleStun()
		{
			var unit = GetSelectedUnit();
			if (unit?.runtime == null)
				return;

			unit.runtime.isStunned = !unit.runtime.isStunned;
			Debug.Log($"[UnitServiceDebugger] {unit.name} Stunned: {unit.runtime.isStunned}");
		}

		[HorizontalGroup("Debug Actions/Status")]
		[Button("Kill Unit", ButtonSizes.Medium), GUIColor(1f, 0.3f, 0.3f)]
		[EnableIf("@HasSelectedUnit && SelectedUnitIsAlive")]
		private void KillUnit()
		{
			var unit = GetSelectedUnit();
			if (unit?.runtime == null)
				return;

			unit.runtime.currentHp = 0;
			Debug.Log($"[UnitServiceDebugger] {unit.name} killed (HP set to 0)");
		}

		[HorizontalGroup("Debug Actions/Status")]
		[Button("Revive Unit", ButtonSizes.Medium), GUIColor(0.3f, 1f, 0.5f)]
		[EnableIf("@HasSelectedUnit && !SelectedUnitIsAlive")]
		private void ReviveUnit()
		{
			var unit = GetSelectedUnit();
			if (unit?.runtime == null || unit.stats == null)
				return;

			unit.runtime.currentHp = unit.stats.maxHp;
			Debug.Log($"[UnitServiceDebugger] {unit.name} revived (HP set to max)");
		}

		private bool SelectedUnitIsAlive
		{
			get
			{
				var unit = GetSelectedUnit();
				return unit?.runtime?.StillAlive ?? false;
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
			if (LevelContainer.Instance == null)
				return;

			_unitService = LevelContainer.Instance.TryResolve<IUnitService>();
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Safely gets the currently selected unit.
		/// Returns null if service is unavailable or unit not found.
		/// </summary>
		private Unit GetSelectedUnit()
		{
			if (_unitService == null)
				return null;

			if (string.IsNullOrEmpty(_selectedUnitId))
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

		private UnitStatsInfo GetSelectedUnitStats()
		{
			var unit = GetSelectedUnit();
			if (unit?.stats == null)
				return default;

			return new UnitStatsInfo
			{
				MaxHp = unit.stats.maxHp,
				Speed = unit.stats.speed,
				MoveRange = unit.stats.moveRange,
				ActionPoints = unit.stats.actionPoints
			};
		}

		private UnitRuntimeInfo GetSelectedUnitRuntime()
		{
			var unit = GetSelectedUnit();
			if (unit == null)
				return default;

			var runtime = unit.runtime;
			var stats = unit.stats;

			// Handle null runtime or stats
			if (runtime == null || stats == null)
				return default;

			var maxHp = stats.maxHp;
			var currentHp = runtime.currentHp;
			var hpPercent = maxHp > 0 ? (float)currentHp / maxHp : 0f;

			return new UnitRuntimeInfo
			{
				CurrentHp = currentHp,
				MaxHp = maxHp,
				HpPercent = hpPercent,
				IsAlive = runtime.StillAlive,
				IsStunned = runtime.isStunned
			};
		}

		#endregion

		#region Display Data Structures

		/// <summary>
		/// Display structure for unit list table.
		/// </summary>
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

		/// <summary>
		/// Display structure for selected unit basic info.
		/// </summary>
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
		/// Display structure for selected unit stats.
		/// </summary>
		[Serializable]
		private struct UnitStatsInfo
		{
			[HorizontalGroup("Row1")]
			[LabelText("Max HP"), LabelWidth(60)]
			public int MaxHp;

			[HorizontalGroup("Row1")]
			[LabelText("Speed"), LabelWidth(50)]
			public int Speed;

			[HorizontalGroup("Row2")]
			[LabelText("Move"), LabelWidth(60)]
			public int MoveRange;

			[HorizontalGroup("Row2")]
			[LabelText("AP"), LabelWidth(50)]
			public int ActionPoints;
		}

		/// <summary>
		/// Display structure for selected unit runtime state.
		/// </summary>
		[Serializable]
		private struct UnitRuntimeInfo
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

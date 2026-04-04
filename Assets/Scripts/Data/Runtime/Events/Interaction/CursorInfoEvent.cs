using Core.Events;
using UnityEngine;

namespace Data.Runtime.Events.Interaction
{
	public readonly struct CursorInfoEvent : IEvent
	{
		// ── Common ──────────────────────────────────────────────
		public Vector2Int? Cell { get; }
		public Vector3 WorldPosition { get; }

		// ── Terrain (present whenever Cell != null) ─────────────
		public string TerrainName { get; }

		// ── Unit info (when hovering a unit) ────────────────────
		public string UnitName { get; }
		public int? UnitHp { get; }
		public int? UnitMaxHp { get; }
		public string UnitFaction { get; }

		// ── Movement context (MovementPreview only) ────────────
		public int? MovementApCost { get; }
		public int? RemainingAp { get; }
		/// <summary>False if the unit can pass through but cannot stop on this cell.</summary>
		public bool? CanStopHere { get; }

		// ── Attack context (AttackPreview only) ─────────────────
		public int? HitChance { get; }

		private CursorInfoEvent(
			Vector2Int? cell, Vector3 worldPosition,
			string terrainName,
			string unitName, int? unitHp, int? unitMaxHp, string unitFaction,
			int? movementApCost, int? remainingAp, bool? canStopHere,
			int? hitChance)
		{
			Cell = cell;
			WorldPosition = worldPosition;
			TerrainName = terrainName;
			UnitName = unitName;
			UnitHp = unitHp;
			UnitMaxHp = unitMaxHp;
			UnitFaction = unitFaction;
			MovementApCost = movementApCost;
			RemainingAp = remainingAp;
			CanStopHere = canStopHere;
			HitChance = hitChance;
		}

		// ── Factory methods ────────────────────────────────────

		/// <summary>Hides the tooltip. Published on state exit or when pointer leaves map.</summary>
		public static CursorInfoEvent Hide() => default;

		/// <summary>Idle / UnitSelected: hovering an empty cell.</summary>
		public static CursorInfoEvent ForTerrain(Vector2Int cell, Vector3 worldPos, string terrainName)
			=> new(cell, worldPos, terrainName,
				null, null, null, null,
				null, null, null,
				null);

		/// <summary>Idle / UnitSelected: hovering a cell occupied by a unit.</summary>
		public static CursorInfoEvent ForUnit(
			Vector2Int cell, Vector3 worldPos, string terrainName,
			string unitName, int hp, int maxHp, string faction)
			=> new(cell, worldPos, terrainName,
				unitName, hp, maxHp, faction,
				null, null, null,
				null);

		/// <summary>MovementPreview: hovering a reachable cell with known path cost.</summary>
		public static CursorInfoEvent ForMovement(
			Vector2Int cell, Vector3 worldPos, string terrainName,
			int apCost, int remainingAp, bool canStopHere)
			=> new(cell, worldPos, terrainName,
				null, null, null, null,
				apCost, remainingAp, canStopHere,
				null);

		/// <summary>AttackPreview: hovering a valid attack target.</summary>
		public static CursorInfoEvent ForAttack(
			Vector2Int cell, Vector3 worldPos, string terrainName,
			string targetName, int targetHp, int targetMaxHp, int hitChance)
			=> new(cell, worldPos, terrainName,
				targetName, targetHp, targetMaxHp, null,
				null, null, null,
				hitChance);

		public override string ToString()
		{
			if (!Cell.HasValue) return "[CursorInfo] Hidden";
			var info = $"[CursorInfo] Cell:{Cell}";
			if (TerrainName != null) info += $" Terrain:{TerrainName}";
			if (UnitName != null) info += $" Unit:{UnitName}";
			if (MovementApCost.HasValue) info += $" AP:{MovementApCost}";
			if (HitChance.HasValue) info += $" Hit:{HitChance}%";
			return info;
		}
	}
}

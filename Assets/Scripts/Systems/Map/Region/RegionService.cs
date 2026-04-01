using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Map;
using Systems.Map.Config;
using UnityEngine;

namespace Systems.Map.Region
{
	public class RegionService : IRegionService
	{
		private readonly IEventBus _eventBus;
		private readonly IMapService _mapService;

		private readonly Dictionary<int, RegionDefinition> _regionDefs = new();	// regionId → definition metadata
		private readonly Dictionary<int, List<Vector2Int>> _regionCells = new(); // regionId → all cell positions in that region
		private readonly Dictionary<Vector2Int, int> _cellToRegion = new(); // cell position → regionId (reverse lookup)
		private readonly Dictionary<int, List<WallKey>> _boundaryWalls = new(); // regionId → precomputed boundary walls
		private readonly HashSet<int> _unlockedRegions = new();

		private Vector2Int _mapSize;
		private bool _initialized;

		private static readonly Vector2Int[] CardinalDirections =
		{
			new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
		};

		public RegionService(IEventBus eventBus, IMapService mapService)
		{
			_eventBus = eventBus;
			_mapService = mapService;
		}

		public void Initialize(MapConfig config)
		{
			_mapSize = config.Size;

			_regionDefs.Clear();
			_regionCells.Clear();
			_cellToRegion.Clear();
			_unlockedRegions.Clear();
			_boundaryWalls.Clear();

			BuildRegionDefinitions(config);
			BuildCellToRegionMapping(config);
			PrecomputeBoundaryWalls();

			_initialized = true;

			// Just log
			foreach (var (id, cells) in _regionCells)
			{
				var def = _regionDefs.GetValueOrDefault(id);
				var wallCount = _boundaryWalls.TryGetValue(id, out var walls) ? walls.Count : 0;
				var status = _unlockedRegions.Contains(id) ? "unlocked" : "locked";
				this.Log($"Region {id} '{def.regionName}': {cells.Count} cells, {wallCount} boundary walls, {status}");
			}
		}

		public void UnlockRegion(int regionId)
		{
			if (!_initialized)
			{
				this.LogError("UnlockRegion called before Initialize.");
				return;
			}

			if (!_regionDefs.ContainsKey(regionId))
			{
				this.LogWarning($"Attempted to unlock undefined region {regionId}.");
				return;
			}

			if (!_unlockedRegions.Add(regionId))
				return;

			var cells = GetRegionCells(regionId);
			var walls = GetRegionBoundaryWalls(regionId);

			this.Log($"Region {regionId} unlocked. Cells: {cells.Count}, Boundary walls: {walls.Count}");

			_eventBus.Publish(new RegionUnlockedEvent(regionId, cells, walls));
		}

		public bool IsRegionUnlocked(int regionId) => _unlockedRegions.Contains(regionId);

		public bool IsCellUnlocked(Vector2Int position)
			=> !_cellToRegion.TryGetValue(position, out var regionId) ||
			   _unlockedRegions.Contains(regionId);

		public IReadOnlyList<Vector2Int> GetRegionCells(int regionId) =>
			_regionCells.TryGetValue(regionId, out var cells)
				? cells
				: Array.Empty<Vector2Int>();

		public IReadOnlyList<WallKey> GetRegionBoundaryWalls(int regionId) =>
			_boundaryWalls.TryGetValue(regionId, out var walls)
				? walls
				: Array.Empty<WallKey>();

		private void BuildRegionDefinitions(MapConfig config)
		{
			if (config.regions == null || config.regions.Length == 0)
			{
				var defaultRegion = RegionDefinition.DefaultOutdoor;
				_regionDefs[defaultRegion.regionId] = defaultRegion;
				_regionCells[defaultRegion.regionId] = new List<Vector2Int>();
				_unlockedRegions.Add(defaultRegion.regionId);
				this.LogWarning("No regions defined in MapConfig, using default outdoor region.");
				return;
			}

			foreach (var def in config.regions)
			{
				if (!_regionDefs.TryAdd(def.regionId, def))
				{
					this.LogWarning($"Duplicate region ID {def.regionId}, skipping '{def.regionName}'.");
					continue;
				}

				_regionCells[def.regionId] = new List<Vector2Int>();

				if (def.initiallyUnlocked)
					_unlockedRegions.Add(def.regionId);
			}
		}

		private void BuildCellToRegionMapping(MapConfig config)
		{
			foreach (var cellConfig in config.cells)
			{
				var pos = cellConfig.position;
				var regionId = cellConfig.regionId;

				_cellToRegion[pos] = regionId;

				if (_regionCells.TryGetValue(regionId, out var cellList))
					cellList.Add(pos);
				else
					this.LogWarning($"Cell {pos} references undefined region {regionId}.");
			}
		}

		private void PrecomputeBoundaryWalls()
		{
			foreach (var (regionId, cells) in _regionCells)
			{
				var boundarySet = new HashSet<WallKey>();

				foreach (var cell in cells)
				{
					foreach (var direction in CardinalDirections)
					{
						var neighbor = cell + direction;

						if (!IsInBounds(neighbor))
							continue;

						if (_cellToRegion.TryGetValue(neighbor, out var neighborRegion) &&
						    neighborRegion == regionId)
							continue;

						boundarySet.Add(new WallKey(cell, neighbor));
					}
				}

				_boundaryWalls[regionId] = new List<WallKey>(boundarySet);
			}
		}

		private bool IsInBounds(Vector2Int pos) =>
			pos.x >= 0 && pos.x < _mapSize.x && pos.y >= 0 && pos.y < _mapSize.y;
	}
}

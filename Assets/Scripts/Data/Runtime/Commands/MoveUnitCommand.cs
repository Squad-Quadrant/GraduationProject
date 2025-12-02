using System.Collections.Generic;
using Core.Commands;
using Core.Events;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.View;
using Systems.Map;
using Systems.Unit;
using UnityEngine;

namespace Data.Runtime.Commands
{
	public class MoveUnitCommand : AsyncCommand
	{
		private const bool EnableLogs = true;

		private readonly string _unitId;
		private readonly Vector2Int _fromPosition;
		private readonly Vector2Int _toPosition;
		private readonly IReadOnlyList<Vector2Int> _path;

		private readonly IUnitService _unitService;
		private readonly IMapService _mapService;
		private readonly IEventBus _eventBus;

		public override string Name => $"Move({_unitId}: {_fromPosition} → {_toPosition})";
		public override bool CanUndo => true;

		public MoveUnitCommand(
			string unitId,
			Vector2Int fromPosition,
			Vector2Int toPosition,
			IReadOnlyList<Vector2Int> path,
			IUnitService unitService,
			IMapService mapService,
			IEventBus eventBus)
		{
			_unitId = unitId;
			_fromPosition = fromPosition;
			_toPosition = toPosition;
			_path = path;
			_unitService = unitService;
			_mapService = mapService;
			_eventBus = eventBus;
		}

		protected override void OnExecuteAsync()
		{
			Log($"[MoveUnitCommand] Executing: {Name}");

			if (!_unitService.TryGetUnit(_unitId, out var unit))
			{
				LogError($"[MoveUnitCommand] Unit '{_unitId}' not found!");
				CompleteExecution();
				return;
			}

			_mapService.ReleaseCell(_fromPosition);
			_mapService.OccupyCell(_toPosition, _unitId);
			unit.position = _toPosition;

			_eventBus.Subscribe<AnimationCompleteEvent>(OnAnimationComplete, onlyOnce: true);

			_eventBus.Publish(new UnitMovedEvent(
				_unitId,
				_fromPosition,
				_toPosition,
				_path
			));

			CompleteExecution(); // todo: just for testing without animation
		}

		protected override void OnUndoAsync()
		{
			Log($"[MoveUnitCommand] Undoing: {Name}");

			if (!_unitService.TryGetUnit(_unitId, out var unit))
			{
				Debug.LogError($"[MoveUnitCommand] Unit '{_unitId}' not found!");
				CompleteUndo();
				return;
			}

			// Reverse the move
			_mapService.ReleaseCell(_toPosition);
			_mapService.OccupyCell(_fromPosition, _unitId);
			unit.position = _fromPosition;

			// Publish reverse movement event
			var reversePath = new List<Vector2Int>(_path);
			reversePath.Reverse();

			_eventBus.Publish(new UnitMovedEvent(
				_unitId,
				_toPosition,
				_fromPosition,
				reversePath
			));

			CompleteUndo(); // todo: just for testing without animation
		}

		private void OnAnimationComplete(AnimationCompleteEvent e)
		{
			// Check if this is our animation
			if (e.EntityId != _unitId || e.AnimationType != EAnimationType.Move)
				return;

			Log($"[MoveUnitCommand] Animation complete for {_unitId}");
			CompleteExecution();
		}

		#region Debug

		private void Log(string message)
		{
			if (EnableLogs) Debug.Log($"{message}");
		}

		private void LogWarning(string message)
		{
			if (EnableLogs) Debug.LogWarning($"{message}");
		}

		private void LogError(string message)
		{
			if (EnableLogs) Debug.LogError($"{message}");
		}

		#endregion
	}
}

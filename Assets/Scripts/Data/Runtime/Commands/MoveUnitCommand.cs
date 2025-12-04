using System;
using System.Collections.Generic;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.View;
using Systems.Map;
using Systems.Unit;
using UnityEngine;

namespace Data.Runtime.Commands
{
	public class MoveUnitCommand : AsyncCommand
	{
		private readonly string _unitId;
		private readonly Vector2Int _fromPosition;
		private readonly Vector2Int _toPosition;
		private readonly IReadOnlyList<Vector2Int> _path;

		private readonly IUnitService _unitService;
		private readonly IMapService _mapService;
		private readonly IEventBus _eventBus;

		public override string Name => $"Move({_unitId}: {_fromPosition} → {_toPosition})";
		public override bool CanUndo => true;

		private Action<AnimationCompleteEvent> _onAnimationComplete;

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
			this.Log($"Executing: {Name}");

			if (!_unitService.TryGetUnit(_unitId, out var unit))
			{
				this.LogError($"Unit '{_unitId}' not found!");
				CompleteExecution();
				return;
			}

			_mapService.ReleaseCell(_fromPosition);
			_mapService.OccupyCell(_toPosition, _unitId);
			unit.position = _toPosition;

			_onAnimationComplete = OnAnimationComplete;
			_eventBus.Subscribe(_onAnimationComplete);

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
			this.Log($"Undoing: {Name}");

			if (!_unitService.TryGetUnit(_unitId, out var unit))
			{
				this.LogError($"Unit '{_unitId}' not found!");
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

			this.Log($"Animation complete for {_unitId}");

			if (_onAnimationComplete != null)
			{
				_eventBus.Unsubscribe(_onAnimationComplete);
				_onAnimationComplete = null;
			}
			CompleteExecution();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly int _apCost;

		private readonly IUnitService _unitService;
		private readonly IMapService _mapService;
		private readonly IEventBus _eventBus;

		public bool WaitForAnimation { get; set; } = true;

		private Action<PresentationCompleteEvent> _onPresentationComplete;

		public override string Name => $"Move ['{_unitId}': {_fromPosition} → {_toPosition}]";
		public override bool CanUndo => false;


		public MoveUnitCommand(
			string unitId,
			Vector2Int fromPosition,
			Vector2Int toPosition,
			IReadOnlyList<Vector2Int> path,
			int apCost,
			IUnitService unitService,
			IMapService mapService,
			IEventBus eventBus)
		{
			_unitId = unitId;
			_fromPosition = fromPosition;
			_toPosition = toPosition;
			_path = path;
			_apCost = apCost;
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

			_mapService.OccupyCell(_toPosition, _unitId);
			unit.position = _toPosition;
			unit.currentAp -= _apCost;

			_eventBus.Publish(new UnitMovedEvent(
				unit,
				_fromPosition,
				_toPosition,
				_path
			));
            
			if (WaitForAnimation)
			{
				_onPresentationComplete = OnPresentationComplete;
				_eventBus.Subscribe(_onPresentationComplete);
			}
			else
            {
                var units = _unitService.GetAllAliveUnits().ToList();
                CompleteExecution();
            }
		}

		// protected override void OnUndoAsync()
		// {
		// 	this.Log($"Undoing: {Name}");
		//
		// 	if (!_unitService.TryGetUnit(_unitId, out var unit))
		// 	{
		// 		this.LogError($"Unit '{_unitId}' not found!");
		// 		CompleteUndo();
		// 		return;
		// 	}
		//
		// 	// Reverse the move
		// 	_mapService.ReleaseCell(_toPosition);
		// 	_mapService.OccupyCell(_fromPosition, _unitId);
		// 	unit.position = _fromPosition;
		// 	unit.currentAp += _apCost;
		//
		// 	// Publish reverse movement event
		// 	var reversePath = new List<Vector2Int>(_path);
		// 	reversePath.Reverse();
		//
		// 	_eventBus.Publish(new UnitMovedEvent(
		// 		unit,
		// 		_toPosition,
		// 		_fromPosition,
		// 		reversePath
		// 	));
		//
		// 	CompleteUndo();
		// }

		private void OnPresentationComplete(PresentationCompleteEvent e)
		{
			// Check if this is our animation
			if (!e.Matches(EPresentationCategory.Animation, PresentationType.Animation.Move, _unitId))
				return;

			this.Log($"Animation complete for {_unitId}");

			_mapService.ReleaseCell(_fromPosition); // 保证移动移动期间的墙壁透明性

			Cleanup();
			CompleteExecution();
		}

		private void Cleanup()
		{
			if (_onPresentationComplete == null) return;
			_eventBus.Unsubscribe(_onPresentationComplete);
			_onPresentationComplete = null;
		}
	}
}

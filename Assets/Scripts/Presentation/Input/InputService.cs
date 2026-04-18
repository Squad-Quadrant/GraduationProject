using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.UI;
using Data.Runtime.Events.Vision;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map;
using Systems.Unit;
using Systems.Vision;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Presentation.Input
{
	public class InputService : MonoBehaviour
	{
		[Title("Dependencies")]
		[SerializeField] private Camera mainCamera;

		[Title("Raycast Settings")]
		[SerializeField] private LayerMask unitLayer;

		[Title("Settings")]
		[ShowInInspector, ReadOnly] private bool _isEnabled;

		private IEventBus _eventBus;
		private ICoordinateConverter _coordinateConverter;
		private IMapService _mapService;
		private IUnitService _unitService;
		private IVisionService _visionService;

		private GameInputActions _inputActions;

		private Vector2Int? _lastHoveredCell;
		private string _lastHoveredUnitId;

		public void Initialize(ServiceContainer services)
		{
			_eventBus = services.Resolve<IEventBus>();
			_coordinateConverter = services.Resolve<ICoordinateConverter>();
			_mapService = services.Resolve<IMapService>();
			_unitService = services.Resolve<IUnitService>();
			_visionService = services.Resolve<IVisionService>();

			if (!mainCamera) mainCamera = Camera.main;

			SetupInputActions();

			this.Log("Initialized");
		}

		private void SetupInputActions()
		{
			_inputActions = new GameInputActions();

			// Subscribe to input callbacks
			_inputActions.Gameplay.PrimaryClick.performed += OnPrimaryClick;
			_inputActions.Gameplay.SecondaryClick.performed += OnSecondaryClick;
			_inputActions.Gameplay.ESC.performed += OnESC;

			SetEnabled(true);
		}

		private void OnDestroy()
		{
			if (_inputActions == null) return;

			_inputActions.Gameplay.PrimaryClick.performed -= OnPrimaryClick;
			_inputActions.Gameplay.SecondaryClick.performed -= OnSecondaryClick;
			_inputActions.Gameplay.ESC.performed -= OnESC;

			_inputActions.Dispose();
			_inputActions = null;
		}

		private void Update()
		{
			if (!_isEnabled || _inputActions == null) return;

			UpdatePointerHover();
		}

		public void SetEnabled(bool enable)
		{
			_isEnabled = enable;

			if (enable)
				_inputActions?.Gameplay.Enable();
			else
				_inputActions?.Gameplay.Disable();

			this.Log($"{(enable ? "Enabled" : "Disabled")}");
		}

		private void OnPrimaryClick(InputAction.CallbackContext ctx)
		{
			if (!_isEnabled) return;

			var screenPos = _inputActions.Gameplay.PointerPosition.ReadValue<Vector2>();
			var worldPos = ScreenToWorldPosition(screenPos);
			var cellPos = _coordinateConverter.WorldToCell(worldPos);

			var unitId = GetUnitAtCell(cellPos);
			if (unitId != null)
			{
				this.Log($"Unit clicked: {unitId} at {cellPos}");

				_eventBus.Publish(new UnitClickedEvent(
					unitId,
					cellPos,
					worldPos
				));

				return;
			}

			if (_mapService.Data.IsInBounds(cellPos))
			{
				this.Log($"Cell clicked: {cellPos}");

				_eventBus.Publish(new CellClickedEvent(
					cellPos,
					worldPos
				));
			}
			else
				this.Log($"Click out of map bounds: {cellPos}");
		}

		private void OnSecondaryClick(InputAction.CallbackContext ctx)
		{
			if (!_isEnabled) return;

			this.Log("Right-click pressed");
			_eventBus.Publish(new BackInputEvent());
		}

		private void OnESC(InputAction.CallbackContext ctx)
		{
			if (!_isEnabled) return;

			this.Log("Esc pressed");
			_eventBus.Publish(new EscInputEvent());
		}

		private void UpdatePointerHover()
		{
			var screenPos = _inputActions.Gameplay.PointerPosition.ReadValue<Vector2>();
			var worldPos = ScreenToWorldPosition(screenPos);
			var cellPos = _coordinateConverter.WorldToCell(worldPos);

			Vector2Int? currentCell = null;
			string currentUnitId = null;

			if (_mapService.Data.IsInBounds(cellPos))
			{
				currentCell = cellPos;
				// currentUnitId = DetectUnitAtPosition(screenPos);
				currentUnitId = GetUnitAtCell(cellPos); // todo: physical detection has not been implemented yet
			}

			// Only publish if something changed
			if (currentCell == _lastHoveredCell && currentUnitId == _lastHoveredUnitId) return;

			_lastHoveredCell = currentCell;
			_lastHoveredUnitId = currentUnitId;

			_eventBus.Publish(new PointerHoverEvent(
				currentCell,
				worldPos,
				currentUnitId
			));

			if (currentCell.HasValue)
			{
				this.LogDebug($"Hover: Cell={currentCell}, Unit={currentUnitId ?? "none"}");
			}
		}

		private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
		{
			var worldPoint = mainCamera.ScreenToWorldPoint(
				new Vector3(screenPosition.x, screenPosition.y, -mainCamera.transform.position.z)
			);
			return new Vector3(worldPoint.x, worldPoint.y, 0f);
		}

		private string DetectUnitAtPosition(Vector2 screenPosition)
		{
			var worldPos = ScreenToWorldPosition(screenPosition);

			// Use OverlapPoint for precise 2D detection at a single point
			var collider2d = Physics2D.OverlapPoint(worldPos, unitLayer);

			if (!collider2d) return null;

			var clickableUnit = collider2d.GetComponentInParent<IClickableUnit>();
			return clickableUnit?.UnitId;
		}

		private string GetUnitAtCell(Vector2Int cellPosition)
		{
			if (!_visionService.IsCellVisible(cellPosition))
				return null;

			return (from unit in _unitService.GetAllUnits()
				where unit.position == cellPosition select unit.id).FirstOrDefault();
		}
	}
}

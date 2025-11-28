using System;
using Core.Events;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.UI;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map;
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
		[SerializeField] private bool enableLogs = true;
		[SerializeField] [ReadOnly] private bool isEnabled;

		private IEventBus _eventBus;
		private ICoordinateConverter _coordinateConverter;
		private IMapService _mapService;

		private GameInputActions _inputActions;

		private Vector2Int? _lastHoveredCell;
		private string _lastHoveredUnitId;

		public void Initialize(IEventBus eventBus, ICoordinateConverter coordinateConverter, IMapService mapService)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_coordinateConverter = coordinateConverter ?? throw new ArgumentNullException(nameof(coordinateConverter));
			_mapService = mapService ?? throw new ArgumentNullException(nameof(mapService));

			if (!mainCamera) mainCamera = Camera.main;

			SetupInputActions();

			Log("[InputService] Initialized");
		}

		private void SetupInputActions()
		{
			_inputActions = new GameInputActions();

			// Subscribe to input callbacks
			_inputActions.Gameplay.PrimaryClick.performed += OnPrimaryClick;
			_inputActions.Gameplay.SecondaryClick.performed += OnSecondaryClick;
			_inputActions.Gameplay.Cancel.performed += OnCancel;

			SetEnabled(true);
		}

		private void OnDestroy()
		{
			if (_inputActions == null) return;

			_inputActions.Gameplay.PrimaryClick.performed -= OnPrimaryClick;
			_inputActions.Gameplay.SecondaryClick.performed -= OnSecondaryClick;
			_inputActions.Gameplay.Cancel.performed -= OnCancel;

			_inputActions.Dispose();
			_inputActions = null;
		}

		private void Update()
		{
			if (!isEnabled || _inputActions == null) return;

			UpdatePointerHover();
		}

		public void SetEnabled(bool enable)
		{
			isEnabled = enable;

			if (enable)
				_inputActions?.Gameplay.Enable();
			else
				_inputActions?.Gameplay.Disable();

			Log($"[InputService] {(enable ? "Enabled" : "Disabled")}");
		}

		private void OnPrimaryClick(InputAction.CallbackContext ctx)
		{
			if (!isEnabled) return;

			var screenPos = _inputActions.Gameplay.PointerPosition.ReadValue<Vector2>();
			ProcessClick(screenPos, 0);
		}

		private void OnSecondaryClick(InputAction.CallbackContext ctx)
		{
			if (!isEnabled) return;

			var screenPos = _inputActions.Gameplay.PointerPosition.ReadValue<Vector2>();
			ProcessClick(screenPos, 1);
		}

		private void OnCancel(InputAction.CallbackContext ctx)
		{
			if (!isEnabled) return;

			Log("[InputService] Cancel pressed");
			_eventBus.Publish(new ActionCancelledEvent());
		}

		private void ProcessClick(Vector2 screenPosition, int mouseButton)
		{
			var worldPos = ScreenToWorldPosition(screenPosition);
			var cellPos = _coordinateConverter.WorldToCell(worldPos);

			// First, check if we hit a unit
			var unitId = DetectUnitAtPosition(screenPosition);
			if (unitId != null)
			{
				Log($"[InputService] Unit clicked: {unitId} at {cellPos}");

				_eventBus.Publish(new UnitClickedEvent(
					unitId,
					cellPos,
					worldPos,
					mouseButton
				));
				return;
			}

			// No unit hit, check if cell is within map bounds
			if (_mapService.Data.IsInBounds(cellPos))
			{
				Log($"[InputService] Cell clicked: {cellPos}");

				_eventBus.Publish(new CellClickedEvent(
					cellPos,
					worldPos,
					mouseButton
				));
			}
			else
				Log($"[InputService] Click out of map bounds: {cellPos}");
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
				currentUnitId = DetectUnitAtPosition(screenPos);
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
				// Log($"[InputService] Hover: Cell={currentCell}, Unit={currentUnitId ?? "none"}");
				Log($"{screenPos} -> {worldPos} -> {cellPos}");
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

			if (collider2d)
			{
				var clickableUnit = collider2d.GetComponent<IClickableUnit>();
				return clickableUnit?.UnitId;
			}

			return null;
		}

		#region Debug

		private void Log(string message)
		{
			if (enableLogs) Debug.Log(message);
		}

		#endregion
	}
}

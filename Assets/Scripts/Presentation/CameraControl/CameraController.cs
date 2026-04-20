using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.View;
using Data.Runtime.Events.Vision;
using DG.Tweening;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map;
using Systems.Unit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Presentation.CameraControl
{
	public class CameraController : MonoBehaviour
	{
		[Title("Reference")]
		[SerializeField, Required] private Camera mainCamera;

		[Title("Configuration")]
		[SerializeField, Required, InlineEditor] private CameraConfig config;

		[Title("Debug")]
		[ShowInInspector, ReadOnly] private bool _isDragging;
		[ShowInInspector, ReadOnly] private float _targetZoom;
		[ShowInInspector, ReadOnly] private bool _isFocusing;
		[ShowInInspector, ReadOnly] private Bounds _worldBounds;
		[ShowInInspector, ReadOnly] private Vector3 _panVelocity;

		private IEventBus _eventBus;
		private ICoordinateConverter _coordinateConverter;
		private IMapService _mapService;
		private IUnitService _unitService;

		private Tween _focusTween;
		private Vector3 _dragWorldOrigin;

		private bool _isInitialized;

		public void Initialize(ServiceContainer services)
		{
			_eventBus = services.Resolve<IEventBus>();
			_coordinateConverter = services.Resolve<ICoordinateConverter>();
			_mapService = services.Resolve<IMapService>();
			_unitService = services.Resolve<IUnitService>();

			_targetZoom = mainCamera.orthographicSize;

			ComputeWorldBounds();
			_eventBus.Subscribe<UnitTurnStartedEvent>(OnUnitTurnStarted);
			_eventBus.Subscribe<EnemiesDiscoveredEvent>(OnEnemiesDiscovered);
			_eventBus.Subscribe<UnitAttackedDealDamageEvent>(OnAttackDealDamage);

			_isInitialized = true;
			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			_eventBus.Unsubscribe<UnitTurnStartedEvent>(OnUnitTurnStarted);
			_eventBus.Unsubscribe<EnemiesDiscoveredEvent>(OnEnemiesDiscovered);
			_eventBus.Unsubscribe<UnitAttackedDealDamageEvent>(OnAttackDealDamage);
			KillFocusTween();
		}

		private void LateUpdate()
		{
			HandleDragInput();
			HandleKeyboardPan();
			HandleZoomInput();
			ApplyZoomSmoothing();
			ClampCameraPosition();
		}

		private void OnUnitTurnStarted(UnitTurnStartedEvent e)
		{
			if (!e.IsVisibleToPlayer)
			{
				_eventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.Camera, PresentationType.Camera.Focus)); // 没有焦点事件，但仍需通知流程继续，防止游戏卡死
				return;
			}

			var worldPos = _coordinateConverter.CellToWorld(e.CellPosition);
			FocusOn(worldPos);
		}

		private void OnEnemiesDiscovered(EnemiesDiscoveredEvent e)
		{
			if (e.DiscoveredUnits == null || e.DiscoveredUnits.Count == 0)
				return;

			var positions = new List<Vector2Int>(e.DiscoveredUnits.Count);
			positions.AddRange(e.DiscoveredUnits.Select(unit => unit.position));

			this.Log($"Enemies discovered: {positions.Count} units, starting focus sequence");

			FocusOnCellSequence(positions);
		}

		private void OnAttackDealDamage(UnitAttackedDealDamageEvent e)
		{
			if (e.Attacker == null) return;

			var worldPos = _coordinateConverter.CellToWorld(e.Attacker.position);
			FocusOn(worldPos);
			this.Log($"Unit attacked: {e.Attacker.id}, focusing on attacker position");
		}

		public void FocusOn(Vector3 worldPos)
		{
			KillFocusTween();

			var target = ClampPosition(worldPos);
			var cameraTransform = mainCamera.transform;
			var targetPos = new Vector3(target.x, target.y, cameraTransform.position.z);

			_isFocusing = true;
			_focusTween = cameraTransform
				.DOMove(targetPos, config.focusDuration)
				.SetEase(config.focusEase)
				.OnComplete(() =>
				{
					_isFocusing = false;
					_eventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.Camera,
						PresentationType.Camera.Focus));
				})
				.OnKill(() =>
				{
					_isFocusing = false;
					_eventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.Camera,
						PresentationType.Camera.Focus));
				});

			this.Log($"Focusing on {worldPos}");
		}

		public void FocusOnCell(Vector2Int cellPos)
		{
			var worldPos = _coordinateConverter.CellToWorld(cellPos);
			FocusOn(worldPos);
		}

		public void FocusOnCellSequence(IReadOnlyList<Vector2Int> cellPositions)
		{
			if (cellPositions == null || cellPositions.Count == 0)
				return;

			KillFocusTween();

			var cameraTransform = mainCamera.transform;
			var seq = DOTween.Sequence();

			for (int i = 0; i < cellPositions.Count; i++)
			{
				var worldPos = _coordinateConverter.CellToWorld(cellPositions[i]);
				var clamped = ClampPosition(worldPos);
				var targetPos = new Vector3(clamped.x, clamped.y, cameraTransform.position.z);

				// move to the target position
				seq.Append(
					cameraTransform
						.DOMove(targetPos, config.discoveryFocusDuration)
						.SetEase(config.discoveryEase)
				);

				// interval
				if (i < cellPositions.Count - 1)
					seq.AppendInterval(config.discoveryDwellDuration);
			}

			_isFocusing = true;
			seq.OnComplete(() =>
				{
					_isFocusing = false;
					_focusTween = null;
					this.Log("Discovery focus sequence complete");
					_eventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.Camera,
						PresentationType.Camera.DiscoveryFocus));
				})
				.OnKill(() =>
				{
					_isFocusing = false;
					_focusTween = null;
					this.Log("Discovery focus sequence complete");
					_eventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.Camera,
						PresentationType.Camera.DiscoveryFocus));
				});

			_focusTween = seq;

			this.Log($"Discovery focus sequence started: {cellPositions.Count} targets");
		}

		public void SnapTo(Vector3 worldPos)
		{
			KillFocusTween();

			var clamped = ClampPosition(worldPos);
			var camTransform = mainCamera.transform;
			camTransform.position = new Vector3(clamped.x, clamped.y, camTransform.position.z);

			this.Log($"Snapped to {worldPos}");
		}

		public void SnapToCell(Vector2Int cellPos)
		{
			var worldPos = _coordinateConverter.CellToWorld(cellPos);
			SnapTo(worldPos);
		}

		public void Shake()
		{
			KillFocusTween();

			mainCamera.transform
				.DOShakePosition(config.shakeDuration, config.shakeStrength)
				.SetUpdate(true);

			this.Log("Shake triggered");
		}

		private void HandleDragInput()
		{
			if (_isFocusing) return;

			var mouse = Mouse.current;
			if (mouse == null) return;

			var boundButton = mouse.middleButton;

			if (boundButton.wasPressedThisFrame)
			{
				KillFocusTween();
				_panVelocity = Vector3.zero;
				_isDragging = true;
				_dragWorldOrigin = ScreenToWorldPosition(mouse.position.ReadValue());
			}

			if (_isDragging && boundButton.isPressed)
			{
				var currentWorldPos = ScreenToWorldPosition(mouse.position.ReadValue());
				var delta = _dragWorldOrigin - currentWorldPos;
				var targetPos = mainCamera.transform.position + delta;
				mainCamera.transform.position = Vector3.Lerp(
					mainCamera.transform.position,
					targetPos,
					Time.deltaTime * config.dragSpeed);
			}

			if (boundButton.wasReleasedThisFrame)
				_isDragging = false;
		}

		private void HandleKeyboardPan()
		{
			if (_isFocusing) return;
			if (_isDragging) return; // 鼠标抓取时屏蔽键盘移动
			var kb = Keyboard.current;
			if (kb == null) return;

			var dir = Vector2.zero;
			if (kb.wKey.isPressed) dir.y += 1f;
			if (kb.sKey.isPressed) dir.y -= 1f;
			if (kb.aKey.isPressed) dir.x -= 1f;
			if (kb.dKey.isPressed) dir.x += 1f;

			var targetVelocity = Vector3.zero;
			if (dir != Vector2.zero)
			{
				KillFocusTween();
				dir.Normalize();
				targetVelocity = dir * (config.panSpeed * mainCamera.orthographicSize);
			}

			_panVelocity = Vector3.Lerp(_panVelocity, targetVelocity, config.panSmoothing * Time.deltaTime);
			if (_panVelocity.sqrMagnitude < 0.0001f)
			{
				_panVelocity = Vector3.zero;
				return;
			}
			mainCamera.transform.position += _panVelocity * Time.deltaTime;
		}

		private void HandleZoomInput()
		{
			var mouse = Mouse.current;
			if (mouse == null) return;

			var scrollDelta = mouse.scroll.ReadValue().y;
			if (Mathf.Abs(scrollDelta) < 0.01f) return;

			var normalized = Mathf.Sign(scrollDelta);

			_targetZoom -= normalized * config.zoomSensitivity;
			_targetZoom = Mathf.Clamp(_targetZoom, config.ZoomMin, config.ZoomMax);
		}

		private void ApplyZoomSmoothing()
		{
			var currentZoom = mainCamera.orthographicSize;

			if (Mathf.Abs(currentZoom - _targetZoom) < 0.01f)
			{
				mainCamera.orthographicSize = _targetZoom;
				return;
			}

			mainCamera.orthographicSize = Mathf.Lerp(
				currentZoom, _targetZoom,
				Time.deltaTime * config.zoomSmoothSpeed);
		}

		private void ComputeWorldBounds()
		{
			var mapSize = _mapService.Data.Size;
			var bottom = _coordinateConverter.CellToWorld(Vector2Int.zero);
			var top = _coordinateConverter.CellToWorld(new Vector2Int(mapSize.x - 1, mapSize.y - 1));
			var left = _coordinateConverter.CellToWorld(new Vector2Int(0, mapSize.y - 1));
			var right = _coordinateConverter.CellToWorld(new Vector2Int(mapSize.x - 1, 0));
			var bottomLeft = new Vector3(left.x, bottom.y, 0);
			var topRight = new Vector3(right.x, top.y, 0);
			var center = (bottomLeft + topRight) * 0.5f;
			var size = topRight - bottomLeft;
			size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 0f);
			_worldBounds = new Bounds(center, size);
			this.Log($"World bounds: center={center}, size={size}");
		}

		private Vector3 ClampPosition(Vector3 position)
		{
			var padding = config.boundaryPadding;
			var min = _worldBounds.min;
			var max = _worldBounds.max;

			return new Vector3(
				Mathf.Clamp(position.x, min.x - padding, max.x + padding),
				Mathf.Clamp(position.y, min.y - padding, max.y + padding),
				position.z);
		}

		private void ClampCameraPosition()
		{
			var pos = mainCamera.transform.position;
			var clamped = ClampPosition(pos);
			if (pos != clamped)
				mainCamera.transform.position = clamped;
		}

		private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
		{
			var worldPoint = mainCamera.ScreenToWorldPoint(
				new Vector3(screenPosition.x, screenPosition.y, -mainCamera.transform.position.z) // 投影到z=0平面上
			);
			return new Vector3(worldPoint.x, worldPoint.y, 0f);
		}

		private void KillFocusTween()
		{
			if (_focusTween == null) return;
			_focusTween.Kill();
			_focusTween = null;
		}

		private void OnDrawGizmos()
		{
			if (!_isInitialized) return;

			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(_worldBounds.center, _worldBounds.size);
			Gizmos.color = Color.yellow;
			var padding = config.boundaryPadding * 2;
			Gizmos.DrawWireCube(_worldBounds.center, new Vector3(_worldBounds.size.x + padding, _worldBounds.size.y + padding, 0f));
		}
	}
}

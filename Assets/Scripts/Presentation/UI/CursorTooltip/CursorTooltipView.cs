using System;
using Core.Events;
using Data.Runtime.Events.Interaction;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = System.Diagnostics.Debug;

namespace Presentation.UI.CursorTooltip
{
	public class CursorTooltipView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required] private CanvasGroup canvasGroup;
		[SerializeField, Required] private TextMeshProUGUI headerText;
		[SerializeField, Required] private TextMeshProUGUI detailText;
		[SerializeField] private Camera mainCamera;

		[Title("Timing")]
		[SerializeField] private float showDelay = 0.4f;

		[Title("Cursor Follow")]
		[SerializeField] private bool followCursor = true;
		[SerializeField] private Vector2 cursorOffset = new(16f, -16f);

		[Title("Positioning")]
		[SerializeField] private Vector3 offset = new(0f, 0.6f, 0f);

		[Title("Colors")]
		[SerializeField] private Color enemyColor = new(1f, 0.4f, 0.3f);
		[SerializeField] private Color allyColor = new(0.4f, 0.8f, 1f);
		[SerializeField] private Color neutralColor = new(0.9f, 0.9f, 0.9f);
		[SerializeField] private Color unreachableColor = new(0.6f, 0.6f, 0.6f);

		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		private ICoordinateConverter _coordinateConverter;
		private ICoordinateConverter CoordinateConverter => _coordinateConverter ??= LevelContainer.Instance.Resolve<ICoordinateConverter>();

		private CursorInfoEvent _pendingEvent;
		private float _pendingSince;
		private bool _isShown;

		private void Awake()
		{
			if (!mainCamera) mainCamera = Camera.main;

			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
			canvasGroup.alpha = 0f;
			_isShown = false;
		}

		private void OnEnable()
		{
			EventBus.Subscribe<CursorInfoEvent>(OnCursorInfo);
		}

		private void OnDisable()
		{
			if (!RootContainer.Instance) return;
			EventBus.Unsubscribe<CursorInfoEvent>(OnCursorInfo);
			ClearPending();
			Hide();
		}

		private void Update()
		{
			if (!_isShown && _pendingEvent.Target != ECursorInfoTarget.None)
			{
				if (Time.unscaledTime - _pendingSince >= showDelay)
				{
					ApplyContent(_pendingEvent);
					Show();
				}
			}

			if (_isShown) UpdatePosition();
		}

		private void OnCursorInfo(CursorInfoEvent e)
		{
			if (e.Target == ECursorInfoTarget.None || !e.Cell.HasValue)
			{
				ClearPending();
				Hide();
				return;
			}

			if (_isShown)
			{
				ApplyContent(e);
				UpdatePosition();
				return;
			}

			_pendingEvent = e;
			_pendingSince = Time.unscaledTime;
		}

		private void ClearPending() => _pendingEvent = default; // Target == None

		private void Show()
		{
			if (_isShown) return;
			_isShown = true;
			canvasGroup.alpha = 1f;
			UpdatePosition();
		}

		private void Hide()
		{
			if (!_isShown && canvasGroup.alpha == 0f) return;
			_isShown = false;
			canvasGroup.alpha = 0f;
		}

		private void UpdatePosition()
		{
			if (!mainCamera) return;
			var mouse = Mouse.current;
			if (mouse == null) return;

			if (followCursor)
			{
				var screenPos = mouse.position.ReadValue();
				var worldPos = mainCamera.ScreenToWorldPoint(
					new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z));
				transform.position = new Vector3(worldPos.x, worldPos.y, 0f) + (Vector3)cursorOffset;
			}
			else
			{
				if (_pendingEvent.Cell == null) return;
				transform.position = CoordinateConverter.CellToWorld(_pendingEvent.Cell.Value) + offset;
			}
		}

		private void ApplyContent(CursorInfoEvent e)
		{
			switch (e.Target)
			{
				case ECursorInfoTarget.Cell:
					BuildCellContent(e);
					break;
				case ECursorInfoTarget.Unit:
					BuildUnitContent(e);
					break;
				case ECursorInfoTarget.Movement:
					BuildMovementContent(e);
					break;
				case ECursorInfoTarget.Attack:
					BuildAttackContent(e);
					break;
				case ECursorInfoTarget.None:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void BuildCellContent(CursorInfoEvent e)
		{
			headerText.text = e.CellStatusLine ?? "";
			headerText.color = Color.white;
			detailText.gameObject.SetActive(false);
		}

		// Target == Unit：单位名 + HP + 护甲；spotted 敌人只显示"已发现敌人"
		private void BuildUnitContent(CursorInfoEvent e)
		{
			if (e.UnitIsSpottedHidden)
			{
				headerText.text = "已发现敌人";
				headerText.color = enemyColor;
				detailText.gameObject.SetActive(false);
				return;
			}

			headerText.text = e.UnitName ?? "";
			headerText.color = GetFactionColor(e.UnitFaction);

			// 护甲为 0 时不显示护甲段（和 UnitInfoPanel 的处理一致）
			detailText.text = e.UnitDefense > 0
				? $"HP: {e.UnitHp}/{e.UnitMaxHp}  护甲: {e.UnitDefense}"
				: $"HP: {e.UnitHp}/{e.UnitMaxHp}";
			detailText.gameObject.SetActive(true);
		}

		// Target == Movement：AP cost + 剩余 AP；不可停时只提示 "Cannot Stop"
		private void BuildMovementContent(CursorInfoEvent e)
		{
			if (e.CanStopHere)
			{
				headerText.text = $"AP Cost: {e.MovementApCost}";
				headerText.color = Color.white;
				detailText.text = $"Remaining: {e.RemainingAp}";
				detailText.gameObject.SetActive(true);
			}
			else
			{
				headerText.text = "Cannot Stop";
				headerText.color = unreachableColor;
				detailText.gameObject.SetActive(false);
			}
		}

		// Target == Attack：命中率 + 目标简要信息
		private void BuildAttackContent(CursorInfoEvent e)
		{
			headerText.text = $"Hit: {e.HitChance}%";
			headerText.color = Color.white;
			detailText.text = $"{e.TargetName}  HP: {e.TargetHp}/{e.TargetMaxHp}";
			detailText.gameObject.SetActive(true);
		}

		private Color GetFactionColor(EUnitFaction faction)
		{
			return faction switch
			{
				EUnitFaction.Enemy   => enemyColor,
				EUnitFaction.Player  => allyColor,
				EUnitFaction.Neutral => neutralColor,
				_                    => Color.white
			};
		}
	}
}

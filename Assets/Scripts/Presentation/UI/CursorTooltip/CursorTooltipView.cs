using System;
using Core.Events;
using Data.Runtime.Events.Interaction;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Interfaces;
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

		private bool _isVisible;

		private void Awake() => Hide();

		private void OnEnable()
		{
			EventBus.Subscribe<CursorInfoEvent>(OnCursorInfo);
		}

		private void OnDisable()
		{
			EventBus.Unsubscribe<CursorInfoEvent>(OnCursorInfo);
			Hide();
		}

		private void OnCursorInfo(CursorInfoEvent e)
		{
			if (!e.Cell.HasValue)
			{
				Hide();
				return;
			}

			UpdateContent(e);
			UpdatePosition(e.Cell.Value);
			Show();
		}

		private void Show()
		{
			if (_isVisible) return;
			_isVisible = true;
			canvasGroup.alpha = 1f;
		}

		private void Hide()
		{
			if (!_isVisible && canvasGroup.alpha == 0f) return;
			_isVisible = false;
			canvasGroup.alpha = 0f;
		}

		private void UpdatePosition(Vector2Int cell)
		{
			var worldPos = CoordinateConverter.CellToWorld(cell);
			transform.position = worldPos + offset;
		}

		private void UpdateContent(CursorInfoEvent e)
		{
			// Movement info
			if (e.MovementApCost.HasValue)
			{
				BuildMovementContent(e);
				return;
			}

			// Attack info
			if (e.HitChance.HasValue)
			{
				BuildAttackContent(e);
				return;
			}

			// Unit info (hovering a unit in Idle/UnitSelected)
			if (e.UnitName != null)
			{
				BuildUnitContent(e);
				return;
			}

			// Terrain only (empty cell in Idle/UnitSelected)
			BuildTerrainContent(e);
		}

		private void BuildMovementContent(CursorInfoEvent e)
		{
			if (e.CanStopHere == true)
			{
				headerText.text = $"AP Cost: {e.MovementApCost}";
				headerText.color = Color.white;
				detailText.text = $"Remaining: {e.RemainingAp}";
				detailText.gameObject.SetActive(true);
			}
			else
			{
				// Path exists but unit can't end turn here (occupied, etc.)
				headerText.text = "Cannot Stop";
				headerText.color = unreachableColor;
				detailText.text = e.TerrainName ?? "";
				detailText.gameObject.SetActive(!string.IsNullOrEmpty(detailText.text));
			}
		}

		private void BuildAttackContent(CursorInfoEvent e)
		{
			headerText.text = $"Hit: {e.HitChance}%";
			headerText.color = Color.white;

			if (e.UnitName != null)
			{
				detailText.text = $"{e.UnitName}  HP: {e.UnitHp}/{e.UnitMaxHp}";
				detailText.gameObject.SetActive(true);
			}
			else
			{
				detailText.gameObject.SetActive(false);
			}
		}

		private void BuildUnitContent(CursorInfoEvent e)
		{
			headerText.text = e.UnitName;
			headerText.color = GetFactionColor(e.UnitFaction);

			detailText.text = $"HP: {e.UnitHp}/{e.UnitMaxHp}";
			detailText.gameObject.SetActive(true);
		}

		private void BuildTerrainContent(CursorInfoEvent e)
		{
			headerText.text = e.TerrainName ?? "";
			headerText.color = Color.white;
			detailText.gameObject.SetActive(false);
		}

		private Color GetFactionColor(string faction)
		{
			return faction switch
			{
				"Enemy"   => enemyColor,
				"Player"  => allyColor,
				"Neutral" => neutralColor,
				_         => Color.white
			};
		}
	}
}

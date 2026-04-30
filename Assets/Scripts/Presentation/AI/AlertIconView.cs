using Core.Events;
using Core.Log;
using Data.Runtime.Events.AI;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.AI.Alert;
using UnityEngine;

namespace Presentation.AI
{
	public class AlertIconView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required, ChildGameObjectsOnly] private SpriteRenderer spriteRenderer;

		[Title("Sprites")]
		[SerializeField, Required] private Sprite alertSprite;
		[SerializeField, Required] private Sprite combatSprite;

		private string _unitId;
		private IEventBus _eventBus;
		private EAlertLevel _currentLevel = EAlertLevel.Calm;
		private bool _visible = true;

		private void Awake() => spriteRenderer.enabled = false;

		public void Bind(string unitId)
		{
			_unitId = unitId;
			_eventBus = RootContainer.Instance.Resolve<IEventBus>();
			_eventBus.Subscribe<UnitAlertStateChangedEvent>(OnAlertChanged);
		}

		private void OnDestroy() => _eventBus?.Unsubscribe<UnitAlertStateChangedEvent>(OnAlertChanged);

		public void SetVisible(bool visible)
		{
			_visible = visible;
			RefreshDisplay();
		}

		private void OnAlertChanged(UnitAlertStateChangedEvent e)
		{
			if (e.UnitId != _unitId) return;
			_currentLevel = e.To;
			RefreshDisplay();
			this.Log($"'{_unitId}' alert icon: {e.To}");
		}

		private void RefreshDisplay()
		{
			bool shouldShow = _visible && _currentLevel != EAlertLevel.Calm;
			spriteRenderer.enabled = shouldShow;

			if (shouldShow)
				spriteRenderer.sprite = _currentLevel == EAlertLevel.Combat ? combatSprite : alertSprite;
		}
	}
}

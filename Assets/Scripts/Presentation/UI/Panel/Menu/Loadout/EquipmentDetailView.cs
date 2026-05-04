using Sirenix.OdinInspector;
using Systems.Unit.Equipment.Config;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Presentation.UI.Panel.Menu.Loadout
{
	public class EquipmentDetailView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required, ChildGameObjectsOnly] private CanvasGroup canvasGroup;
		[SerializeField, Required, ChildGameObjectsOnly] private TextMeshProUGUI detailName;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailLine1;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailLine2;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailLine3;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailLine4;
		[SerializeField, ChildGameObjectsOnly] private TextMeshProUGUI detailDescription;

		[Title("Cursor Follow")]
		[SerializeField, Tooltip("相对鼠标的像素偏移。X>0 在光标右侧，Y<0 在光标下方，避免 tooltip 遮挡光标。")]
		private Vector2 cursorOffset = new(16f, -16f);

		[SerializeField, Tooltip("离屏幕边缘的最小距离 (像素)。贴边时 tooltip 会反向回推。")]
		private float edgePadding = 8f;

		private RectTransform _rectTransform;
		private Canvas _canvas;

		private readonly Vector3[] _worldCornerBuf = new Vector3[4];

		private bool _isShown;

		private void Awake()
		{
			_rectTransform = (RectTransform)transform;
			_canvas = GetComponentInParent<Canvas>();

			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;

			canvasGroup.alpha = 0f;
			_isShown = false;
		}

		public void Show(EquipmentConfig config)
		{
			if (!config)
			{
				Hide();
				return;
			}

			ClearText();
			detailName.text = config.nName;
			detailDescription.text = config.description;
			switch (config)
			{
				case WeaponConfig weaponConfig:
					detailLine1.text = $"伤害：　　{weaponConfig.damage}";
					detailLine2.text = $"弹容量：　{weaponConfig.ammoCapacity}发";
					detailLine3.text = $"射速：　　{weaponConfig.shootSpeed}发/AP";
					detailLine4.text = $"穿透率：　{weaponConfig.penetrationRate:P1}";
					break;
				case TacticalItemConfig tacticalItemConfig:
					detailLine1.text = $"使用次数：　{tacticalItemConfig.maxUsesPerBattle}次";
					detailLine2.text = $"AP消耗：　　{tacticalItemConfig.apCost}点/次";
					break;
			}

			_rectTransform.SetAsLastSibling();

			UpdatePosition();

			canvasGroup.alpha = 1f;
			_isShown = true;
		}

		public void Hide()
		{
			if (!_isShown) return;
			canvasGroup.alpha = 0f;
			_isShown = false;
		}

		private void Update()
		{
			if (_isShown) UpdatePosition();
		}

		private void UpdatePosition()
		{
			var mouse = Mouse.current;
			if (mouse == null || !_canvas) return;

			Vector2 screenPos = mouse.position.ReadValue();
			Vector2 target = screenPos + cursorOffset;

			_rectTransform.GetWorldCorners(_worldCornerBuf);
			Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
			Vector2 blScreen = RectTransformUtility.WorldToScreenPoint(cam, _worldCornerBuf[0]);
			Vector2 trScreen = RectTransformUtility.WorldToScreenPoint(cam, _worldCornerBuf[2]);
			Vector2 sizeInScreen = trScreen - blScreen;

			Vector2 pivot = _rectTransform.pivot;

			float overflowRight = (target.x + (1f - pivot.x) * sizeInScreen.x) - (Screen.width - edgePadding);
			if (overflowRight > 0f) target.x -= overflowRight;
			float overflowLeft = edgePadding - (target.x - pivot.x * sizeInScreen.x);
			if (overflowLeft > 0f) target.x += overflowLeft;

			float overflowTop = (target.y + (1f - pivot.y) * sizeInScreen.y) - (Screen.height - edgePadding);
			if (overflowTop > 0f) target.y -= overflowTop;
			float overflowBottom = edgePadding - (target.y - pivot.y * sizeInScreen.y);
			if (overflowBottom > 0f) target.y += overflowBottom;

			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, target, cam, out Vector3 worldPoint))
				_rectTransform.position = worldPoint;
		}

		private void ClearText()
		{
			detailName.text = "";
			detailDescription.text = "";
			detailLine1.text = "";
			detailLine2.text = "";
			detailLine3.text = "";
			detailLine4.text = "";
		}
	}
}

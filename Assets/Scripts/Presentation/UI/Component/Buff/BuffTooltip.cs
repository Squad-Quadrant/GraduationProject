using Systems.Buff;
using TMPro;
using UnityEngine;

namespace Presentation.UI.Component.Buff
{
	public class BuffTooltip : MonoBehaviour
	{
		[SerializeField] private GameObject root;
		[SerializeField] private TextMeshProUGUI titleText;
		[SerializeField] private TextMeshProUGUI descriptionText;
		[SerializeField] private Vector2 anchorOffset = new(0f, 12f); // 相对图标的偏移

		private RectTransform _rect;

		private void Awake()
		{
			_rect = (RectTransform)transform;
			Hide();
		}

		public void Show(BuffInfo info, RectTransform anchor)
		{
			if (info == null) return;

			var data = info.BuffData;
			titleText.text = info.CurrentStack > 1
				? $"{data.buffName} ×{info.CurrentStack}"
				: data.buffName;
			descriptionText.text = data.description;

			if (anchor) _rect.position = anchor.position + (Vector3)anchorOffset;
			root.SetActive(true);
		}

		public void Hide()
		{
			if (root)
				root.SetActive(false);
		}
	}
}

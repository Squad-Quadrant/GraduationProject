using Presentation.Audio;
using Presentation.Audio.Config;
using Presentation.Bootstrap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Presentation.UI.Component
{
	[RequireComponent(typeof(Button))]
	public class UIButtonSfx : MonoBehaviour, IPointerEnterHandler
	{
		[SerializeField] private EUISfx onClickSfx = EUISfx.ButtonClickBattle;
		[SerializeField] private EUISfx onHoverSfx = EUISfx.ButtonHover;

		private Button _button;
		private AudioService _audioService;

		private void Awake()
		{
			_audioService = RootContainer.Instance.TryResolve<AudioService>();

			_button = GetComponent<Button>();
			_button.onClick.AddListener(OnClick);
		}

		private void OnDestroy()
		{
			if (_button) _button.onClick.RemoveListener(OnClick);
		}

		private void OnClick()
		{
			if (onClickSfx == EUISfx.None || !_audioService) return;
			_audioService.PlayUISfx(onClickSfx);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (onHoverSfx == EUISfx.None || !_audioService) return;
			_audioService.PlayUISfx(onHoverSfx);
		}
	}
}

using System;
using Core.Log;
using DG.Tweening;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    public class TurnBannerPanel : UIPanel
    {
	    [TitleGroup("References")]
	    [SerializeField, Required] private TextMeshProUGUI titleText;

	    [TitleGroup("References")]
	    [SerializeField, Required] private TextMeshProUGUI subtitleText;

	    [TitleGroup("References")]
	    [SerializeField, Required] private RectTransform bannerRect;

	    [TitleGroup("Animation")]
	    [SerializeField, Range(0.05f, 1f)] private float fadeInDuration = 0.3f;

	    [TitleGroup("Animation")]
	    [SerializeField, Range(0.2f, 5f)] private float holdDuration = 1.0f;

	    [TitleGroup("Animation")]
	    [SerializeField, Range(0.05f, 1f)] private float fadeOutDuration = 0.25f;

	    [TitleGroup("Animation")]
	    [SerializeField] private float slideOffset = 80f;

	    private Vector2 _restPosition;
	    private Sequence _activeSequence;

	    protected override void OnInitialize() => _restPosition = bannerRect.anchoredPosition;

	    protected override void OnOpen()
	    {
		    CanvasGroup.alpha = 0f;
		    CanvasGroup.blocksRaycasts = false;
	    }

	    protected override void OnClose() => KillSequence();

	    protected override void OnDestroy()
	    {
		    KillSequence();
		    base.OnDestroy();
	    }

	    public void Show(string title, string subtitle, Action onComplete)
	    {
		    KillSequence();

		    titleText.text = title;

		    if (subtitleText)
		    {
			    var hasSubtitle = !string.IsNullOrEmpty(subtitle);
			    subtitleText.gameObject.SetActive(hasSubtitle);
			    if (hasSubtitle) subtitleText.text = subtitle;
		    }

		    _activeSequence = BuildSequence(onComplete);
		    this.Log($"Showing banner: '{title}' / '{subtitle ?? "(none)"}'");
	    }

	    private Sequence BuildSequence(Action onComplete)
	    {
		    var slideStart = _restPosition + Vector2.up * slideOffset;

		    CanvasGroup.alpha = 0f;
		    bannerRect.anchoredPosition = slideStart;

		    var seq = DOTween.Sequence();

		    seq.Append(
			    bannerRect
				    .DOAnchorPos(_restPosition, fadeInDuration)
				    .SetEase(Ease.OutCubic));
		    seq.Join(
			    CanvasGroup
				    .DOFade(1f, fadeInDuration)
				    .SetEase(Ease.OutCubic));

		    seq.AppendInterval(holdDuration);

		    seq.Append(
			    CanvasGroup
				    .DOFade(0f, fadeOutDuration)
				    .SetEase(Ease.InCubic));

		    seq.OnComplete(() =>
		    {
			    _activeSequence = null;
			    bannerRect.anchoredPosition = _restPosition;
			    onComplete?.Invoke();
		    });

		    seq.SetUpdate(true);
		    seq.SetAutoKill(false);
		    seq.Play();
		    return seq;
	    }

	    private void KillSequence()
	    {
		    if (_activeSequence == null) return;

		    _activeSequence.Kill(complete: false);
		    _activeSequence = null;

		    CanvasGroup.alpha = 0f;
		    bannerRect.anchoredPosition = _restPosition;
	    }
    }
}

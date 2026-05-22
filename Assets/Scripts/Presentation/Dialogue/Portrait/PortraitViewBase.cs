using System;
using DG.Tweening;
using Presentation.Dialogue.Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Dialogue.Portrait
{
	public abstract class PortraitViewBase : MonoBehaviour, IPortraitView
    {
        [TitleGroup("References")]
        [SerializeField, Required, ChildGameObjectsOnly]
        protected CanvasGroup canvasGroup;

        [SerializeField, Required, ChildGameObjectsOnly]
        protected RectTransform rect;

        [TitleGroup("Animation")]
        [SerializeField, Required]
        protected DialogueAnimationSettings animationSettings;

        protected CharacterConfig Character;
        private Vector2 _restPosition;
        private string _currentPoseName;
        private string _currentSkinName;
        private Tween _activeTween;

        public void Setup(CharacterConfig character, string poseName, string skinName)
        {
            Character = character;
            _currentPoseName = poseName;
            _currentSkinName = skinName;

            rect.anchoredPosition = Vector2.zero;
            _restPosition = Vector2.zero;

            canvasGroup.alpha = 0f;

            ApplyIdle(_currentPoseName, _currentSkinName);
        }

        public void PlayEntrance(EPortraitPosition position, string entryAnimation, Action onComplete)
        {
            KillActiveTween();
            ApplyEntranceStartState(position);

            if (!string.IsNullOrEmpty(entryAnimation))
                PlayOneShotThenIdle(entryAnimation, _currentPoseName, _currentSkinName);

            if (Character.entranceStyle == EEntranceStyle.Cut)
            {
                onComplete?.Invoke();
                return;
            }

            _activeTween = BuildEntranceSequence(onComplete);
        }

        public void PlayExit(EPortraitPosition position, Action onComplete)
        {
            KillActiveTween();

            if (Character.exitStyle == EExitStyle.Cut)
            {
                canvasGroup.alpha = 0f;
                onComplete?.Invoke();
                return;
            }

            _activeTween = BuildExitSequence(position, onComplete);
        }

        public void ChangeAppearance(string poseName, string skinName, string entryAnimation, Action onComplete)
        {
            _currentPoseName = poseName;
            _currentSkinName = skinName;

            if (!string.IsNullOrEmpty(entryAnimation))
                PlayOneShotThenIdle(entryAnimation, _currentPoseName, _currentSkinName);
            else
                ApplyIdle(_currentPoseName, _currentSkinName);

            onComplete?.Invoke();
        }

        protected abstract void ApplyIdle(string poseName, string skinName);

        protected abstract void PlayOneShotThenIdle(string oneShotAnim, string followingPose, string followingSkin);

        private void ApplyEntranceStartState(EPortraitPosition position)
        {
            switch (Character.entranceStyle)
            {
                case EEntranceStyle.Cut:
                    canvasGroup.alpha = 1f;
                    rect.anchoredPosition = _restPosition;
                    break;
                case EEntranceStyle.Fade:
                    canvasGroup.alpha = 0f;
                    rect.anchoredPosition = _restPosition;
                    break;
                case EEntranceStyle.SlideFromSide:
                    canvasGroup.alpha = 1f;
                    rect.anchoredPosition = _restPosition + GetSlideOffset(position);
                    break;
                case EEntranceStyle.FadeWithSlight:
                    canvasGroup.alpha = 0f;
                    rect.anchoredPosition = _restPosition - Vector2.up * animationSettings.slightOffset;
                    break;
                default:
	                throw new ArgumentOutOfRangeException();
            }
        }

        private Sequence BuildEntranceSequence(Action onComplete)
        {
            var settings = animationSettings;
            var seq = DOTween.Sequence();

            switch (Character.entranceStyle)
            {
                case EEntranceStyle.Fade:
                    seq.Append(canvasGroup.DOFade(1f, settings.fadeDuration).SetEase(settings.fadeEase));
                    break;
                case EEntranceStyle.SlideFromSide:
                    seq.Append(rect.DOAnchorPos(_restPosition, settings.slideDuration).SetEase(settings.slideEase));
                    break;
                case EEntranceStyle.FadeWithSlight:
                    seq.Append(canvasGroup.DOFade(1f, settings.fadeDuration).SetEase(settings.fadeEase));
                    seq.Join(rect.DOAnchorPos(_restPosition, settings.fadeDuration).SetEase(settings.fadeEase));
                    break;
                case EEntranceStyle.Cut:
	                break;
                default:
	                throw new ArgumentOutOfRangeException();
            }

            seq.OnComplete(() =>
            {
                _activeTween = null;
                onComplete?.Invoke();
            });
            seq.OnKill(() =>
            {
                if (_activeTween == seq) _activeTween = null;
            });

            return seq;
        }

        private Sequence BuildExitSequence(EPortraitPosition position, Action onComplete)
        {
            var s = animationSettings;
            var seq = DOTween.Sequence();

            switch (Character.exitStyle)
            {
                case EExitStyle.Fade:
                    seq.Append(canvasGroup.DOFade(0f, s.fadeDuration).SetEase(s.fadeEase));
                    break;
                case EExitStyle.SlideToSide:
                    seq.Append(rect.DOAnchorPos(_restPosition + GetSlideOffset(position), s.slideDuration).SetEase(s.slideEase));
                    break;
                case EExitStyle.FadeWithSlight:
                    seq.Append(canvasGroup.DOFade(0f, s.fadeDuration).SetEase(s.fadeEase));
                    seq.Join(rect.DOAnchorPos(_restPosition - Vector2.up * s.slightOffset, s.fadeDuration).SetEase(s.fadeEase));
                    break;
                case EExitStyle.Cut:
	                break;
                default:
	                throw new ArgumentOutOfRangeException();
            }

            seq.OnComplete(() =>
            {
                _activeTween = null;
                onComplete?.Invoke();
            });
            seq.OnKill(() =>
            {
                if (_activeTween == seq) _activeTween = null;
            });

            return seq;
        }

        private Vector2 GetSlideOffset(EPortraitPosition position)
        {
            var dist = animationSettings.slideDistance;
            return position switch
            {
                EPortraitPosition.Left   => Vector2.left  * dist,
                EPortraitPosition.Right  => Vector2.right * dist,
                EPortraitPosition.Center => Vector2.down  * dist,
                _ => Vector2.zero
            };
        }

        private void KillActiveTween()
        {
            if (_activeTween == null) return;
            if (_activeTween.IsActive()) _activeTween.Kill(complete: false);
            _activeTween = null;
        }

        protected virtual void OnDestroy() => KillActiveTween();
    }
}

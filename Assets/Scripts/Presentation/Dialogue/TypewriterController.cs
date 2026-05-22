using System;
using System.Collections.Generic;
using Presentation.Audio;
using Sirenix.OdinInspector;
using TMPEffects.CharacterData;
using TMPEffects.Components;
using UnityEngine;

namespace Presentation.Dialogue
{
	public class TypewriterController : MonoBehaviour
	{
		[TitleGroup("References")]
        [SerializeField, Required, ChildGameObjectsOnly]
        private TMPWriter writer;

        [TitleGroup("Typing")]
        [LabelText("打字速度 (字符/秒)")]
        [SerializeField, Range(5f, 120f)]
        private float charsPerSecond = 30f;

        [LabelText("打字音效")]
        [SerializeField]
        [InfoBox("每一个字符随机选一个播一次")]
        private List<AudioClip> typingSfx;

        private AudioService _audioService;
        private Action _onComplete;

        public bool IsWriting => writer && writer.IsWriting;

        public void Initialize(AudioService audioService) => _audioService = audioService;

        private void Awake()
        {
            writer.WriteOnStart = false;
            writer.WriteOnNewText = false;

            writer.OnFinishWriter.AddListener(HandleFinish);
            writer.OnCharacterShown.AddListener(HandleCharacterShown);
        }

        private void OnDestroy()
        {
	        if (!writer) return;
	        writer.OnFinishWriter.RemoveListener(HandleFinish);
	        writer.OnCharacterShown?.RemoveListener(HandleCharacterShown);
        }

        public void PlayText(string text, Action onComplete)
        {
            _onComplete = onComplete;
            writer.DefaultDelays.delay = 1f / charsPerSecond;

            if (writer.IsWriting) writer.StopWriter();
            writer.SetText(text);
            writer.ResetWriter();
            writer.StartWriter();
        }

        public void SkipToEnd()
        {
            if (!writer.IsWriting) return;
            writer.SkipWriter(skipShowAnimation: true);
        }

        public void Stop()
        {
            if (writer.IsWriting) writer.StopWriter();
            _onComplete = null;
        }

        private void HandleFinish(TMPWriter w)
        {
            var onComplete = _onComplete;
            _onComplete = null;
            onComplete?.Invoke();
        }

        private void HandleCharacterShown(TMPWriter w, CharData charData)
        {
	        if (typingSfx == null) return;
	        if (!_audioService) return;
	        if (!charData.info.isVisible) return;

	        var sfx = typingSfx[UnityEngine.Random.Range(0, typingSfx.Count)];
	        _audioService.PlaySfx(sfx);
        }
	}
}

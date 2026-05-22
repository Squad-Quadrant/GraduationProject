using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Presentation.Audio;
using Presentation.CameraControl;
using Sirenix.OdinInspector;
using TMPEffects.Components;
using TMPEffects.Tags;
using TMPEffects.TMPEvents;
using UnityEngine;

namespace Presentation.Dialogue
{
    public class DialogueTagDispatcher : MonoBehaviour
    {
        [Serializable]
        public class SfxEntry
        {
            [LabelText("标签名")]
            public string name;

            [LabelText("音效 Clip"), PreviewField(40)]
            public AudioClip clip;
        }

        [TitleGroup("References")]
        [SerializeField, Required, ChildGameObjectsOnly]
        private TMPWriter writer;

        [TitleGroup("Sfx Library")]
        [InfoBox("对话中可用的音效。策划在台词里写 <?sfx=name> 时按 name 查找。", InfoMessageType.None)]
        [SerializeField]
        [TableList(ShowIndexLabels = false, AlwaysExpanded = true, DrawScrollView = false)]
        private List<SfxEntry> sfxLibrary = new();

        private AudioService _audioService;
        private CameraController _cameraController;
        private bool _initialized;

        public void Initialize(AudioService audioService, CameraController cameraController)
        {
            _audioService = audioService;
            _cameraController = cameraController;

            if (_initialized) return;

            writer.OnTextEvent.AddListener(HandleTextEvent);
            _initialized = true;
        }

        private void OnDestroy()
        {
            if (_initialized && writer && writer.OnTextEvent != null)
                writer.OnTextEvent.RemoveListener(HandleTextEvent);
        }

        private void HandleTextEvent(TMPEventArgs args)
        {
            var tagName = args.Tag.Name;
            switch (tagName.ToLowerInvariant())
            {
                case "sfx":
                    HandleSfx(args.Tag);
                    break;
                case "shake":
                    HandleShake();
                    break;
                default:
                    this.LogWarning($"Unknown dialogue event tag '<?{tagName}>'. Ignored.");
                    break;
            }
        }

        private void HandleSfx(TMPEffectTag sfxTag)
        {
            if (!_audioService)
            {
                this.LogError("AudioService not injected — DialogueTagDispatcher.Initialize was not called.");
                return;
            }

            if (!TryGetTagValue(sfxTag, out var sfxName))
            {
                this.LogWarning("<?sfx> tag missing value. Use <?sfx=name> or <?sfx name=xxx>.");
                return;
            }

            var clip = LookupSfx(sfxName);
            if (!clip)
            {
                this.LogWarning($"Sfx '{sfxName}' not found in sfxLibrary.");
                return;
            }

            _audioService.PlaySfx(clip);
        }

        private void HandleShake()
        {
            if (!_cameraController)
            {
                this.LogWarning("CameraController not injected — DialogueTagDispatcher.Initialize was not called.");
                return;
            }
            _cameraController.Shake();
        }

        private static bool TryGetTagValue(TMPEffectTag tag, out string value)
        {
            value = null;
            if (tag.Parameters == null) return false;

            if (tag.Parameters.TryGetValue("", out var v) && !string.IsNullOrEmpty(v) ||
                tag.Parameters.TryGetValue("name", out v) && !string.IsNullOrEmpty(v))
            {
                value = v;
                return true;
            }
            return false;
        }

        private AudioClip LookupSfx(string sfxName) =>
	        (from e in sfxLibrary where e.name == sfxName select e.clip).FirstOrDefault();
    }
}

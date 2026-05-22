using System;
using Core.Log;
using Presentation.Audio;
using Presentation.CameraControl;
using Presentation.Dialogue.Config;
using Presentation.Dialogue.Portrait;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Presentation.Dialogue
{
	public struct DialoguePanelData
	{
		public DialogueConfig Config;
		public AudioService Audio;
		public CameraController Camera;
		public Action OnComplete;
		public int StartIndex;
	}

	public class DialoguePanel: UIPanel, IInitializable<DialoguePanelData>
	{
		[TitleGroup("Components")]
        [SerializeField, Required, ChildGameObjectsOnly]
        private PortraitStage portraitStage;

        [SerializeField, Required, ChildGameObjectsOnly]
        private TypewriterController typewriter;

        [SerializeField, Required, ChildGameObjectsOnly]
        private DialogueTagDispatcher tagDispatcher;


        [TitleGroup("Speaker Name")]
        [SerializeField, Required, ChildGameObjectsOnly]
        private GameObject speakerNameContainer;

        [SerializeField, Required, ChildGameObjectsOnly]
        private TMP_Text speakerNameText;

        private enum State
        {
	        Idle, PlayingTransition, Typing, WaitingForAdvance
        }

        [TitleGroup("Runtime")]
        [ShowInInspector, ReadOnly]
        private State _state = State.Idle;

        [ShowInInspector, ReadOnly]
        private int _currentIndex = -1;

        private DialoguePanelData _data;

        public int CurrentIndex => _currentIndex;
        public int NodeCount => _data.Config.nodes.Count;
        public string CurrentStateName => _state.ToString();

        public void DataInitialize(DialoguePanelData data)
        {
            _data = data;
            typewriter.Initialize(_data.Audio);
            tagDispatcher.Initialize(_data.Audio, _data.Camera);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            if (!_data.Config || _data.Config.nodes == null || _data.Config.nodes.Count == 0)
            {
                this.LogWarning("DialoguePanel opened with empty config. Completing immediately.");
                FinishDialogue();
                return;
            }

            _currentIndex = -1;
            _state = State.Idle;

            var startIndex = _data.StartIndex;
            if (startIndex < 0 || startIndex >= _data.Config.nodes.Count)
            {
	            this.LogWarning($"Invalid StartIndex {startIndex}, falling back to 0.");
	            startIndex = 0;
            }

            PlayNode(startIndex);
        }

        protected override void OnClose()
        {
            typewriter.Stop();
            _state = State.Idle;
            base.OnClose();
        }

        private void PlayNode(int index)
        {
            if (index >= _data.Config.nodes.Count)
            {
                FinishDialogue();
                return;
            }

            _currentIndex = index;
            var node = _data.Config.nodes[index];

            _state = State.PlayingTransition;

            portraitStage.Apply(node.portraits, () =>
            {
                ApplySpeakerName(node.speaker);

                _state = State.Typing;
                typewriter.PlayText(node.text, OnTypewriterFinished);
            });
        }

        private void OnTypewriterFinished() => _state = State.WaitingForAdvance;

        private void AdvanceToNext()
        {
            if (_state != State.WaitingForAdvance) return;
            PlayNode(_currentIndex + 1);
        }

        private void FinishDialogue()
        {
            _state = State.PlayingTransition;
            portraitStage.ClearAll(() =>
            {
                _state = State.Idle;
                _currentIndex = -1;
                _data.OnComplete?.Invoke();
            });
        }

        private void ApplySpeakerName(CharacterConfig speaker)
        {
	        if (!speaker)
		        speakerNameContainer.SetActive(false);
	        else
	        {
                speakerNameContainer.SetActive(true);
                speakerNameText.text = speaker.displayName;
            }
        }

        private void Update()
        {
            if (!IsOpen) return;
            if (_state != State.Typing && _state != State.WaitingForAdvance) return;

            if (!IsAdvancePressed()) return;

            if (_state == State.Typing) // 打字途中按键
	            typewriter.SkipToEnd();
            else // State.WaitingForAdvance
	            AdvanceToNext();
        }

        private static bool IsAdvancePressed()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;
            return false;
        }
	}
}

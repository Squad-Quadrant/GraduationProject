using System;
using Core.Log;
using Presentation.Audio;
using Presentation.CameraControl;
using Presentation.Dialogue.Config;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Dialogue
{
	public class DialogueController : MonoBehaviour
	{
		private UIManager _uiManager;
        private AudioService _audioService;
        private CameraController _cameraController;

        private DialoguePanel _currentPanel;
        private Action _externalCallback;

        [ShowInInspector, ReadOnly] private string _currentDialogueId;
        [ShowInInspector, ReadOnly] public bool IsPlaying => _currentPanel;

        public void Initialize(UIManager uiManager, AudioService audioService, CameraController cameraController)
        {
            _uiManager = uiManager;
            _audioService = audioService;
            _cameraController = cameraController;

            this.Log("Initialized");
        }

        public void Play(DialogueConfig config, Action onComplete = null, int startIndex = 0)
        {
            if (!config)
            {
                this.LogError("Play called with null config; firing onComplete immediately.");
                onComplete?.Invoke();
                return;
            }

            if (IsPlaying)
            {
                this.LogError($"Already playing dialogue '{_currentDialogueId}'. Rejecting new Play for '{config.dialogueId}'.");
                onComplete?.Invoke();
                return;
            }

            if (config.nodes == null || config.nodes.Count == 0)
            {
	            this.LogWarning($"Dialogue '{config.dialogueId}' has no nodes; firing onComplete immediately.");
	            onComplete?.Invoke();
	            return;
            }

            if (startIndex < 0 || startIndex >= config.nodes.Count)
            {
	            this.LogError($"Invalid startIndex {startIndex} for dialogue '{config.dialogueId}' (nodes.Count={config.nodes.Count}). Resetting to 0.");
	            startIndex = 0;
            }

            _currentDialogueId = config.dialogueId;
            _externalCallback = onComplete;

            this.Log($"Playing dialogue '{config.dialogueId}' from node {startIndex} ({config.nodes.Count} nodes total)");

            var data = new DialoguePanelData
            {
	            Config = config,
	            Audio = _audioService,
	            Camera = _cameraController,
	            OnComplete = HandleDialogueComplete,
	            StartIndex = startIndex,
            };

            _currentPanel = _uiManager.Open<DialoguePanel, DialoguePanelData>(data);

            if (_currentPanel) return;
            this.LogError($"UIManager.Open<DialoguePanel> returned null. Check UIPanelConfig.");
            var callback = _externalCallback;
            _externalCallback = null;
            _currentDialogueId = null;
            callback?.Invoke();
        }

        public void Stop()
        {
	        if (!IsPlaying) return;

	        var panelToClose = _currentPanel;
	        var stoppedId = _currentDialogueId;
	        var hadCallback = _externalCallback != null;

	        _currentPanel = null;
	        _externalCallback = null;
	        _currentDialogueId = null;

	        if (panelToClose)
		        _uiManager.Close(panelToClose);

	        if (hadCallback)
		        this.LogWarning($"Dialogue '{stoppedId}' stopped before completion. External callback discarded.");
	        else
		        this.Log($"Dialogue stopped: {stoppedId}");
        }

        private void HandleDialogueComplete()
        {
            var panelToClose = _currentPanel;
            var callback = _externalCallback;
            var finishedId = _currentDialogueId;

            _currentPanel = null;
            _externalCallback = null;
            _currentDialogueId = null;

            if (panelToClose)
                _uiManager.Close(panelToClose);

            this.Log($"Dialogue completed: {finishedId}");

            callback?.Invoke();
        }

#if UNITY_EDITOR
        [TitleGroup("Editor Test")]
        [LabelText("测试用对话")]
        [SerializeField] private DialogueConfig testDialogue;

        [TitleGroup("Editor Test")]
        [LabelText("起始节点 Index")]
        [PropertyRange(0, "TestStartIndexMax")]
        [SerializeField] private int testStartIndex;

        private int TestStartIndexMax =>
            testDialogue && testDialogue.nodes is { Count: > 0 }
                ? testDialogue.nodes.Count - 1
                : 0;

        [TitleGroup("Editor Test")]
        [HorizontalGroup("Editor Test/Buttons")]
        [Button("▶ Play", ButtonSizes.Large)]
        [EnableIf("@UnityEngine.Application.isPlaying && !IsPlaying && testDialogue != null")]
        [GUIColor(0.5f, 0.9f, 1f)]
        private void EditorPlay()
        {
	        Play(testDialogue, null, testStartIndex);
        }

        [TitleGroup("Editor Test")]
        [HorizontalGroup("Editor Test/Buttons")]
        [Button("■ Stop", ButtonSizes.Large)]
        [EnableIf("@UnityEngine.Application.isPlaying && IsPlaying")]
        [GUIColor(1f, 0.6f, 0.6f)]
        private void EditorStop() => Stop();

        [TitleGroup("Runtime State")]
        [ShowInInspector, ReadOnly, LabelText("Current Dialogue")]
        private string DebugDialogueId => _currentDialogueId ?? "—";

        [TitleGroup("Runtime State")]
        [ShowInInspector, ReadOnly, LabelText("Current Node")]
        private string DebugNodeInfo => !_currentPanel ? "—" : $"{_currentPanel.CurrentIndex + 1} / {_currentPanel.NodeCount}";

        [TitleGroup("Runtime State")]
        [ShowInInspector, ReadOnly, LabelText("State")]
        private string DebugState => _currentPanel ? _currentPanel.CurrentStateName : "—";
#endif
	}
}

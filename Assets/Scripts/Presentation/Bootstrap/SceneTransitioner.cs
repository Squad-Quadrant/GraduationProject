using System;
using System.Collections;
using Core.Events;
using Core.Log;
using Data.Runtime.Events;
using Presentation.Audio;
using Presentation.UI.Core;
using Presentation.UI.Panel.SceneTransition;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Presentation.Bootstrap
{
	public class SceneTransitioner : MonoBehaviour
	{
		[Title("Settings")]
		[SerializeField, Range(0.1f, 2f)]
		private float fadeDuration = 0.4f;

		[SerializeField, Range(0.1f, 2f)]
		private float bgmFadeDuration = 0.3f;

		[SerializeField]
		[Range(0, 5)] private int extraWaitFrames = 1;

		[ShowInInspector, ReadOnly] private bool _isTransitioning;

		private UIManager _uiManager;
		private AudioService _audioService;
		private IEventBus _eventBus;
		private bool _initialized;

		public bool IsTransitioning => _isTransitioning;

		public void Initialize()
		{
			if (_initialized) return;
			_initialized = true;

			_uiManager = RootContainer.Instance.Resolve<UIManager>();
			_audioService = RootContainer.Instance.Resolve<AudioService>();
			_eventBus = RootContainer.Instance.Resolve<IEventBus>();

			this.Log("Initialized");
		}

		public void LoadScene(string sceneName, bool waitForLevelLoaded)
		{
			if (string.IsNullOrEmpty(sceneName))
			{
				this.LogError("Cannot load scene: name is null/empty");
				return;
			}

			if (_isTransitioning)
			{
				this.LogWarning($"Already transitioning, ignoring request to load '{sceneName}'");
				return;
			}

			if (!_initialized)
			{
				this.LogError("Cannot load scene: SceneTransitioner not initialized");
				return;
			}

			StartCoroutine(LoadSceneRoutine(sceneName, waitForLevelLoaded));
		}

		private IEnumerator LoadSceneRoutine(string sceneName, bool waitForLevelLoaded)
		{
			_isTransitioning = true;
			this.Log($"==== Transition begin: -> {sceneName} ====", format: false);

			var panel = _uiManager.Open<SceneTransitionPanel>();
			if (!panel)
			{
				this.LogError("SceneTransitionPanel could not be opened. Aborting transition.");
				_isTransitioning = false;
				yield break;
			}

			bool fadeInDone = false;
			panel.FadeToBlack(fadeDuration, () => fadeInDone = true);
			_audioService.StopBGM(bgmFadeDuration);
			while (!fadeInDone) yield return null;

			bool levelReady = !waitForLevelLoaded;
			Action<LevelLoadedEvent> handler = null;
			if (waitForLevelLoaded)
			{
				handler = OnLevelLoadedDuringTransition;
				_eventBus.Subscribe(handler);

				void OnLevelLoadedDuringTransition(LevelLoadedEvent e)
				{
					this.Log($"LevelLoadedEvent received during transition: {e}");
					levelReady = true;
				}
			}

			var op = SceneManager.LoadSceneAsync(sceneName);
			if (op == null)
			{
				this.LogError($"LoadSceneAsync returned null for '{sceneName}'. Check Build Settings.");
				if (handler != null) _eventBus.Unsubscribe(handler);
				_isTransitioning = false;
				yield break;
			}

			yield return op;

			while (!levelReady) yield return null;
			if (handler != null) _eventBus.Unsubscribe(handler);

			for (int i = 0; i < extraWaitFrames; i++) yield return null;

			bool fadeOutDone = false;
			panel.FadeFromBlack(fadeDuration, () => fadeOutDone = true);
			while (!fadeOutDone) yield return null;

			_uiManager.Close(panel);

			_isTransitioning = false;
			this.Log($"==== Transition end: -> {sceneName} ====", format: false);
		}
	}
}

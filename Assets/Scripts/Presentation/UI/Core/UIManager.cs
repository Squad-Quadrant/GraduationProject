using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Config;
using Data.Runtime.Events.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Presentation.UI.Core
{
	public class UIManager : MonoBehaviour
	{
		[TitleGroup("Configuration")]
		[SerializeField, Required, InlineEditor]
		private UISettings settings;

		[TitleGroup("Configuration")]
		[SerializeField, Required]
		[Tooltip("The overlay canvas that persists across scenes (DDOL).")]
		private Canvas overlayCanvas;

		[TitleGroup("Configuration")]
		[SerializeField, Required]
		private InputSystemUIInputModule inputModule;

		private UINavigator _navigator;
		private UIFactory _factory;
		private IEventBus _eventBus;

		// Scene-owned canvas references (registered by LevelCanvasProvider)
		private Canvas _screenCanvas;
		private Canvas _worldCanvas;

		private readonly List<UIPanel> _independentPanels = new();

		public UINavigator Navigator => _navigator;
		public bool HasOpenPanels => _navigator?.Count > 0;
		public UIPanel TopPanel => _navigator?.TopPanel;

		public void Initialize(IEventBus eventBus)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_navigator = new UINavigator();
			_factory = new UIFactory(settings, layer =>
			{
				return layer switch
				{
					EUICanvasLayer.Overlay => overlayCanvas?.transform,
					EUICanvasLayer.Screen => _screenCanvas?.transform,
					EUICanvasLayer.World => _worldCanvas?.transform,
					_ => null
				};
			});

			// Subscribe
			// if (inputModule) inputModule.cancel.action.performed += OnCancelInput;
			SceneManager.sceneUnloaded += OnSceneUnloaded;

			_factory.PreloadPanels();
			DontDestroyOnLoad(gameObject);
			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			// if (inputModule) inputModule.cancel.action.performed -= OnCancelInput;
			SceneManager.sceneUnloaded -= OnSceneUnloaded;

			_navigator?.Clear();
			_factory?.ClearCache();
			ClearIndependentPanels();

			this.Log("Destroyed");
		}

		#region Callbacks

		private void OnCancelInput(InputAction.CallbackContext ctx)
		{
			if (!settings.EnableEscNavigation) return;

			if (!_navigator.HandleBack(out var panelToClose)) return;
			if (panelToClose) Close(panelToClose);
		}

		private void OnSceneUnloaded(Scene scene)
		{
			// Clean up null references from destroyed scene objects
			_navigator.CleanupDestroyedPanels();
			_factory.CleanupDestroyedPanels();
			_independentPanels.RemoveAll(p => !p);
			this.Log($"Cleaned up after: {scene.name}");
		}

		#endregion

		#region Open

		public T Open<T>() where T : UIPanel
		{
			var config = settings.GetConfig<T>();
			if (!config)
			{
				this.LogError($"No config for: {typeof(T).Name}");
				return null;
			}

			var panel = _factory.Acquire(config);
			return OpenInternal(panel, config) as T;
		}

		public TPanel Open<TPanel, TData>(TData data) where TPanel : UIPanel, IInitializable<TData>
		{
			var config = settings.GetConfig<TPanel>();
			if (!config)
			{
				this.LogError($"No config for: {typeof(TPanel).Name}");
				return null;
			}

			var panel = _factory.Acquire<TPanel, TData>(data);
			return OpenInternal(panel, config) as TPanel;
		}

		public T OpenNew<T>() where T : UIPanel
		{
			var config = settings.GetConfig<T>();
			if (!config)
			{
				this.LogError($"No config for: {typeof(T).Name}");
				return null;
			}

			var panel = _factory.Create<T>();
			return OpenInternal(panel, config) as T;
		}

		public TPanel OpenNew<TPanel, TData>(TData data) where TPanel : UIPanel, IInitializable<TData>
		{
			var config = settings.GetConfig<TPanel>();
			if (!config)
			{
				this.LogError($"No config for: {typeof(TPanel).Name}");
				return null;
			}

			var panel = _factory.Create<TPanel, TData>(data);
			return OpenInternal(panel, config) as TPanel;
		}

		private UIPanel OpenInternal(UIPanel panel, UIPanelConfig config)
		{
			if (!panel) return null;

			panel.gameObject.SetActive(true);

			if (config.ManagedByStack)
			{
				// Stack-managed: handle focus transitions
				if (_navigator.Contains(panel))
				{
					this.LogWarning($"Already in stack: {panel.PanelId}");
					return panel;
				}

				// Previous top loses focus
				var previousTop = _navigator.TopPanel;
				if (previousTop)
				{
					previousTop.DoUnfocus();
					if (previousTop.HideWhenCovered)
						previousTop.DoHide();
				}

				_navigator.Push(panel);
				panel.DoOpen(panel.DoFocus);
			}
			else
			{
				// Independent: just open, no stack
				if (!_independentPanels.Contains(panel))
					_independentPanels.Add(panel);
				panel.DoOpen();
			}

			_eventBus?.Publish(new PanelOpenedEvent(panel));
			return panel;
		}

		#endregion

		#region Close

		public void Close(UIPanel panel)
		{
			if (!panel) return;

			var config = settings.GetConfig(panel.PanelId);

			if (panel.ManagedByStack && _navigator.Contains(panel))
			{
				bool wasTop = _navigator.TopPanel == panel;
				_navigator.Remove(panel);

				panel.DoClose(() =>
				{
					_factory.Release(panel, config);
					_eventBus?.Publish(new PanelClosedEvent(panel));

					if (_navigator.IsEmpty)
						_eventBus?.Publish(new AllPanelsClosedEvent());
				});

				// Restore focus to new top
				if (!wasTop || !_navigator.TopPanel) return;

				var newTop = _navigator.TopPanel;
				if (newTop.HideWhenCovered)
					newTop.DoShow(() => newTop.DoFocus());
				else
					newTop.DoFocus();
			}
			else
			{
				// Independent panel
				_independentPanels.Remove(panel);
				panel.DoClose(() =>
				{
					_factory.Release(panel, config);
					_eventBus?.Publish(new PanelClosedEvent(panel));
				});
			}
		}

		public void CloseTop()
		{
			if (_navigator.TopPanel) Close(_navigator.TopPanel);
		}

		public void CloseAll()
		{
			while (_navigator.Count > 0)
			{
				var panel = _navigator.TopPanel;
				if (!panel) continue;

				_navigator.Remove(panel);
				panel.DoCloseImmediate();
				var config = settings.GetConfig(panel.PanelId);
				_factory.Release(panel, config);
			}
			_eventBus?.Publish(new AllPanelsClosedEvent());
			this.Log("Closed all stack panels");
		}

		public void Close<T>() where T : UIPanel
		{
			var panel = GetPanel<T>();
			if (panel)
				Close(panel);
		}

		private void ClearIndependentPanels()
		{
			foreach (var panel in _independentPanels.Where(panel => panel))
				Destroy(panel.gameObject);
			_independentPanels.Clear();
		}

		#endregion

		public T GetPanel<T>() where T : UIPanel
		{
			// Search stack first
			var stackPanel = _navigator.Find<T>();
			if (stackPanel) return stackPanel;

			// Search independent panels
			foreach (var p in _independentPanels)
				if (p is T typed)
					return typed;

			return null;
		}

		public bool IsOpen<T>() where T : UIPanel => GetPanel<T>();

		public bool IsOpen(UIPanel panel) => _navigator.Contains(panel) || _independentPanels.Contains(panel);

		public void RegisterCanvas(EUICanvasLayer layer, Canvas canvas)
		{
			switch (layer)
			{
				case EUICanvasLayer.Screen:
					_screenCanvas = canvas;
					this.Log($"Registered Screen canvas: {canvas.name}");
					break;
				case EUICanvasLayer.World:
					_worldCanvas = canvas;
					this.Log($"Registered World canvas: {canvas.name}");
					break;
			}
		}

		public void UnregisterCanvas(EUICanvasLayer layer)
		{
			switch (layer)
			{
				case EUICanvasLayer.Screen:
					_screenCanvas = null;
					this.Log("Unregistered Screen canvas");
					break;
				case EUICanvasLayer.World:
					_worldCanvas = null;
					this.Log("Unregistered World canvas");
					break;
			}
		}

		#region Debug

        [TitleGroup("Runtime Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("Panel Stack Size")]
        private int PanelCount => _navigator?.Count ?? 0;

        [TitleGroup("Runtime Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("Top Panel")]
        private string TopPanelName => _navigator?.TopPanel?.PanelId ?? "None";

        [TitleGroup("Runtime Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("Cached Panels")]
        private int CachedPanelCount => _factory?.CachedCount ?? 0;

        [TitleGroup("Runtime Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("Screen Canvas")]
        private string ScreenCanvasName => _screenCanvas != null ? _screenCanvas.name : "Not registered";

        [TitleGroup("Runtime Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("World Canvas")]
        private string WorldCanvasName => _worldCanvas != null ? _worldCanvas.name : "Not registered";

        [TitleGroup("Runtime Status")]
        [ShowInInspector, ReadOnly]
        [LabelText("Panel Stack")]
        private List<string> PanelStackDisplay
        {
            get
            {
                if (_navigator == null) return new List<string> { "Not initialized" };
                var panels = _navigator.GetAllPanels();
                var names = new List<string>();
                for (int i = 0; i < panels.Count; i++)
                {
                    var prefix = i == panels.Count - 1 ? "▶ " : "  ";
                    names.Add($"{prefix}{panels[i]?.PanelId ?? "null"}");
                }
                return names.Count > 0 ? names : new List<string> { "Empty" };
            }
        }

        [TitleGroup("Debug Actions")]
        [HorizontalGroup("Debug Actions/Row1")]
        [Button("Close Top"), GUIColor(1f, 0.7f, 0.3f)]
        [EnableIf("@UnityEngine.Application.isPlaying && PanelCount > 0")]
        private void DebugCloseTop() => CloseTop();

        [HorizontalGroup("Debug Actions/Row1")]
        [Button("Close All"), GUIColor(1f, 0.4f, 0.3f)]
        [EnableIf("@UnityEngine.Application.isPlaying && PanelCount > 0")]
        private void DebugCloseAll() => CloseAll();

        [TitleGroup("Debug Actions")]
        [HorizontalGroup("Debug Actions/Row2")]
        [Button("Clear Cache"), GUIColor(0.8f, 0.8f, 0.4f)]
        [EnableIf("@UnityEngine.Application.isPlaying")]
        private void DebugClearCache() => _factory?.ClearCache();

        [HorizontalGroup("Debug Actions/Row2")]
        [Button("Cleanup Null Refs"), GUIColor(0.7f, 0.9f, 0.7f)]
        [EnableIf("@UnityEngine.Application.isPlaying")]
        private void DebugCleanup()
        {
            _navigator?.CleanupDestroyedPanels();
            _factory?.CleanupDestroyedPanels();
        }

        #endregion
	}
}

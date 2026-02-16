using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Config;
using Data.Runtime.Events.UI;
using Sirenix.OdinInspector;
using UnityEngine;
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

		private IEventBus _eventBus;

		// Scene-owned canvas references (registered by LevelCanvasProvider)
		private Canvas _screenCanvas;
		private Canvas _worldCanvas;

		private readonly Dictionary<string, UIPanel> _openPanels = new();
		private readonly Dictionary<string, UIPanel> _cache = new(); // closed but not destroyed panels

		public bool HasOpenPanels => _openPanels.Count > 0;

		public bool IsOpen<T>() where T : UIPanel
		{
			var config = settings.GetConfig<T>();
			return config && _openPanels.ContainsKey(config.PanelId);
		}

		public T GetPanel<T>() where T : UIPanel
		{
			var config = settings.GetConfig<T>();
			if (config && _openPanels.TryGetValue(config.PanelId, out var panel))
				return panel as T;
			return null;
		}

		public void Initialize(IEventBus eventBus)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			DontDestroyOnLoad(gameObject);
			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
			CloseAll();
			ClearCache();
			this.Log("Destroyed");
		}

		private void OnSceneUnloaded(Scene scene)
		{
			CleanupNullEntries(_openPanels, "open");
			CleanupNullEntries(_cache, "cached");
			this.Log($"Cleaned up after scene: {scene.name}");
		}

		public TPanel Open<TPanel>() where TPanel : UIPanel
		{
			var config = settings.GetConfig<TPanel>();
			if (!config)
			{
				this.LogError($"No config for: {typeof(TPanel).Name}");
				return null;
			}

			if (_openPanels.TryGetValue(config.PanelId, out var existing) && existing)
			{
				this.Log($"Already open, returning existing: {config.PanelId}");
				return existing as TPanel;
			}

			var panel = GetPanelInstance(config);
			if (!panel) return null;

			RegisterAndOpen(panel);
			return panel as TPanel;
		}

		public TPanel Open<TPanel, TData>(TData data) where TPanel : UIPanel, IInitializable<TData>
		{
			var config = settings.GetConfig<TPanel>();
			if (!config)
			{
				this.LogError($"No config for: {typeof(TPanel).Name}");
				return null;
			}

			if (_openPanels.TryGetValue(config.PanelId, out var existing) && existing is TPanel typedExisting)
			{
				typedExisting.Initialize(data);
				this.Log($"Already open, updated data: {config.PanelId}");
				return typedExisting;
			}

			var panel = GetPanelInstance(config);
			if (panel is not TPanel typedPanel) return null;

			typedPanel.Initialize(data);
			RegisterAndOpen(panel);
			return typedPanel;
		}

		public void Close(UIPanel panel)
		{
			if (!panel) return;

			_openPanels.Remove(panel.Config.PanelId);

			panel.Close(() =>
			{
				ReleasePanel(panel);
				_eventBus?.Publish(new PanelClosedEvent(panel));

				if (_openPanels.Count == 0)
					_eventBus?.Publish(new AllPanelsClosedEvent());
			});
		}

		public void Close<T>() where T : UIPanel
		{
			var panel = GetPanel<T>();
			if (panel) Close(panel);
		}

		public void CloseAll()
		{
			var panelIds = _openPanels.Keys.ToList();

			foreach (var id in panelIds)
			{
				if (!_openPanels.TryGetValue(id, out var panel) || !panel) continue;

				_openPanels.Remove(id);
				panel.CloseImmediate();
				ReleasePanel(panel);
			}

			_openPanels.Clear();
			_eventBus?.Publish(new AllPanelsClosedEvent());
			this.Log("Closed all panels");
		}

		private UIPanel GetPanelInstance(UIPanelConfig config)
		{
			if (_cache.TryGetValue(config.PanelId, out var cached) && cached)
			{
				_cache.Remove(config.PanelId);
				this.Log($"Cache hit: {config.PanelId}");
				return cached;
			}

			_cache.Remove(config.PanelId);
			return InstantiatePanel(config);
		}

		private void ReleasePanel(UIPanel panel)
		{
			if (!panel) return;

			if (panel.Config && panel.Config.CacheOnClose)
			{
				_cache[panel.Config.PanelId] = panel;
				panel.gameObject.SetActive(false);
				this.Log($"Cached: {panel.Config.PanelId}");
			}
			else
			{
				_cache.Remove(panel.Config.PanelId);
				Destroy(panel.gameObject);
				this.Log($"Destroyed: {panel.Config.PanelId}");
			}
		}

		private void RegisterAndOpen(UIPanel panel)
		{
			_openPanels[panel.Config.PanelId] = panel;
			panel.gameObject.SetActive(true);
			panel.Open(() => _eventBus.Publish(new PanelOpenedEvent(panel)));
			this.Log($"Opened: {panel.Config.PanelId}");
		}

		private UIPanel InstantiatePanel(UIPanelConfig config)
		{
			if (!config.Prefab)
			{
				this.LogError($"Cannot instantiate panel: prefab is null ({config.PanelId})");
				return null;
			}

			var parent = GetCanvasRoot(config.Layer);
			if (!parent)
			{
				this.LogError($"Cannot instantiate panel: no canvas for layer {config.Layer} ({config.PanelId})");
				return null;
			}

			var panel = Instantiate(config.Prefab, parent);
			panel.name = config.PanelId;
			panel.Initialize(config);
			this.Log($"Instantiated: {config.PanelId}");
			return panel;
		}

		private void ClearCache()
		{
			foreach (var panel in _cache.Values.Where(p => p))
				Destroy(panel.gameObject);
			_cache.Clear();
		}

		private void CleanupNullEntries(Dictionary<string, UIPanel> dict, string label)
		{
			var staleKeys = dict.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
			foreach (var key in staleKeys)
				dict.Remove(key);

			if (staleKeys.Count > 0)
				this.Log($"Cleaned {staleKeys.Count} destroyed {label} panel(s)");
		}

		#region Canvas Management

		private Transform GetCanvasRoot(EUICanvasLayer layer) => layer switch
		{
			EUICanvasLayer.Overlay => overlayCanvas ? overlayCanvas.transform : null,
			EUICanvasLayer.Screen  => _screenCanvas ? _screenCanvas.transform : null,
			EUICanvasLayer.World   => _worldCanvas  ? _worldCanvas.transform  : null,
			_ => null
		};

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

		#endregion

		#region Debug

		[TitleGroup("Runtime Status")]
		[ShowInInspector, ReadOnly, LabelText("Open Panels")]
		private int OpenPanelCount => _openPanels.Count;

		[TitleGroup("Runtime Status")]
		[ShowInInspector, ReadOnly, LabelText("Cached Panels")]
		private int CachedPanelCount => _cache.Count;

		[TitleGroup("Runtime Status")]
		[ShowInInspector, ReadOnly, LabelText("Screen Canvas")]
		private string ScreenCanvasName => _screenCanvas ? _screenCanvas.name : "Not registered";

		[TitleGroup("Runtime Status")]
		[ShowInInspector, ReadOnly, LabelText("World Canvas")]
		private string WorldCanvasName => _worldCanvas ? _worldCanvas.name : "Not registered";

		[TitleGroup("Runtime Status")]
		[ShowInInspector, ReadOnly, LabelText("Open Panel List")]
		private List<string> OpenPanelDisplay
		{
			get
			{
				if (_openPanels.Count == 0) return new List<string> { "(none)" };
				return _openPanels.Select(kvp =>
					$"{kvp.Key} {(kvp.Value ? "✓" : "✗ null")}").ToList();
			}
		}

		[TitleGroup("Debug Actions")]
		[HorizontalGroup("Debug Actions/Row")]
		[Button("Close All"), GUIColor(1f, 0.4f, 0.3f)]
		[EnableIf("@UnityEngine.Application.isPlaying && _openPanels.Count > 0")]
		private void DebugCloseAll() => CloseAll();

		[HorizontalGroup("Debug Actions/Row")]
		[Button("Clear Cache"), GUIColor(0.8f, 0.8f, 0.4f)]
		[EnableIf("@UnityEngine.Application.isPlaying")]
		private void DebugClearCache() => ClearCache();

		#endregion
	}
}

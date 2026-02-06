using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Data.Config;
using UnityEngine;

namespace Presentation.UI.Core
{
	public class UIFactory
	{
		private readonly UISettings _settings;
		private readonly Func<EUICanvasLayer, Transform> _getCanvasRoot;
		private readonly Dictionary<string, UIPanel> _cache = new();

		public int CachedCount => _cache.Count;

		public UIFactory(UISettings settings, Func<EUICanvasLayer, Transform> getCanvasRoot)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_getCanvasRoot = getCanvasRoot ?? throw new ArgumentNullException(nameof(getCanvasRoot));
		}

		public T Acquire<T>() where T : UIPanel
		{
			var config = _settings.GetConfig<T>();
			if (config)
				return Acquire(config) as T;

			this.LogError($"No config for: {typeof(T).Name}");
			return null;
		}

		public UIPanel Acquire(UIPanelConfig config)
		{
			if (!config)
			{
				this.LogError("Cannot create panel: config is null");
				return null;
			}

			if (_cache.TryGetValue(config.PanelId, out var cached) && cached)
			{
				this.Log($"Cache hit: {config.PanelId}");
				return cached;
			}
			_cache.Remove(config.PanelId);
			return InstantiatePanel(config);
		}

		public TPanel Acquire<TPanel, TData>(TData data) where TPanel : UIPanel, IInitializable<TData>
		{
			var panel = Acquire<TPanel>();
			panel?.Initialize(data);
			return panel;
		}

		public void Release(UIPanel panel, UIPanelConfig config)
		{
			if (!panel) return;

			if (config && config.cacheOnClose)
			{
				// Cache for reuse
				_cache[config.PanelId] = panel;
				panel.gameObject.SetActive(false);
				this.Log($"Cached: {config.PanelId}");
			}
			else
			{
				_cache.Remove(panel.PanelId);
				UnityEngine.Object.Destroy(panel.gameObject);
				this.Log($"Destroyed: {panel.PanelId}");
			}
		}

		public void ClearCache()
		{
			foreach (var panel in _cache.Values.Where(panel => panel))
				UnityEngine.Object.Destroy(panel.gameObject);
			_cache.Clear();
			this.Log("Cache cleared");
		}

		public void CleanupDestroyedPanels()
		{
			var keysToRemove = (from kvp in _cache where !kvp.Value select kvp.Key).ToList();

			foreach (var key in keysToRemove)
				_cache.Remove(key);

			if (keysToRemove.Count > 0)
				this.Log($"Cleaned {keysToRemove.Count} destroyed panel(s) from cache");
		}

		public void PreloadPanels()
		{
			int count = 0;
			foreach (var config in _settings.GetPreloadConfigs())
			{
				if (_cache.ContainsKey(config.PanelId)) continue;

				var panel = InstantiatePanel(config);
				if (!panel) continue;

				_cache[config.PanelId] = panel;
				panel.gameObject.SetActive(false);
				count++;
			}
			if (count > 0)
				this.Log($"Preloaded {count} panel(s)");
		}

		private UIPanel InstantiatePanel(UIPanelConfig config)
		{
			if (!config.Prefab)
			{
				this.LogError($"Prefab is null: {config.PanelId}");
				return null;
			}

			var canvasRoot = _getCanvasRoot(config.Layer);
			if (!canvasRoot)
			{
				this.LogError($"No canvas for {config.Layer}: {config.PanelId}");
				return null;
			}

			var panel = UnityEngine.Object.Instantiate(config.Prefab, canvasRoot);
			panel.name = config.PanelId;
			panel.Initialize(config);

			this.Log($"Created: {config.PanelId}");
			return panel;
		}
	}
}

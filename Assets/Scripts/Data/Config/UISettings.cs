using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.Config
{
	[CreateAssetMenu(fileName = "UISettings", menuName = "Game/UI/UI Settings")]
	public class UISettings : ScriptableObject
	{
		[TitleGroup("Panel Registry")]
		[SerializeField, InlineEditor]
		[ListDrawerSettings(ShowFoldout = true, DraggableItems = true)]
		[Tooltip("All panel configurations in the game.")]
		private List<UIPanelConfig> panelConfigs = new();

		public IReadOnlyList<UIPanelConfig> PanelConfigs => panelConfigs;

		// Runtime lookup dictionary, built on first access
		private Dictionary<string, UIPanelConfig> _lookup;

		private Dictionary<string, UIPanelConfig> Lookup
		{
			get
			{
				if (_lookup != null) return _lookup;

				_lookup = new Dictionary<string, UIPanelConfig>();
				foreach (var config in
				         panelConfigs
					         .Where(config => config)
					         .Where(config => !_lookup.TryAdd(config.PanelId, config)))
					this.LogWarning($"Duplicate panel ID: {config.PanelId}");
				return _lookup;
			}
		}

		public UIPanelConfig GetConfig(string panelId)
			=> Lookup.GetValueOrDefault(panelId);

		public UIPanelConfig GetConfig<T>() where T : UIPanel
			=> panelConfigs.FirstOrDefault(config => config?.Prefab && config.Prefab.GetType() == typeof(T));

		public IEnumerable<UIPanelConfig> GetPreloadConfigs()
			=> panelConfigs.Where(config => config && config.preload);

		private void OnValidate() => _lookup = null;

		[TitleGroup("Debug")]
		[Button("Validate All"), GUIColor(0.4f, 0.8f, 1f)]
		private void ValidateAll()
		{
			var ids = new HashSet<string>();
			int issues = 0;

			foreach (var config in panelConfigs)
			{
				if (!config || !ids.Add(config.PanelId)) { issues++; continue; }
				if (!config.Prefab) { issues++; }
			}

			Debug.Log(issues == 0
				? $"[UISettings] {panelConfigs.Count} panels validated ✓"
				: $"[UISettings] Found {issues} issue(s)");
		}
	}
}

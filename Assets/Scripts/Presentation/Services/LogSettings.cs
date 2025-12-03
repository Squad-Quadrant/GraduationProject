using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Services
{
	[CreateAssetMenu(fileName = "LogSettings", menuName = "Game/LogSettings", order = 0)]
	public class LogSettings : ScriptableObject
	{
		[Title("Global")]
		[EnumToggleButtons]
		[SerializeField] private LogLevel globalLogLevel = LogLevel.Info;
		[SerializeField] private bool enableAllLogs = true;

		[Title("Rules")]
		[TableList(AlwaysExpanded = true)]
		[SerializeField] private List<TypeLogRule> rules = new();

		private Dictionary<string, TypeLogRule> _cache;

		private void OnEnable() => BuildCache();

#if UNITY_EDITOR
		private void OnValidate() => BuildCache();
#endif

		public bool IsEnabled(Type type, LogLevel level)
		{
			if (type == null || !enableAllLogs || level < globalLogLevel)
				return false;

			var rule = FindRuleForType(type);
			if (rule != null)
				return rule.enabled && level >= rule.minLogLevel;

			RecordDiscoveredType(type);
			return true;
		}

		private void BuildCache()
		{
			_cache = new Dictionary<string, TypeLogRule>();

			foreach (var rule in rules.Where(rule => !string.IsNullOrEmpty(rule.typeName)))
				_cache[rule.typeName] = rule;
		}


		private TypeLogRule FindRuleForType(Type type)
		{
			var fullName = type.FullName ?? type.Name;

			if (_cache.TryGetValue(fullName, out var rule))
				return rule;

			return
				!string.IsNullOrEmpty(type.Namespace) ?
					FindNamespaceRule(type.Namespace) :
					null;
		}

		private TypeLogRule FindNamespaceRule(string namespaceName)
		{
			var currentNamespace = namespaceName;

			while (!string.IsNullOrEmpty(currentNamespace))
			{
				if (_cache.TryGetValue($"{currentNamespace}.*", out var rule))
					return rule;

				var lastDot = currentNamespace.LastIndexOf('.');
				if (lastDot < 0) break;
				currentNamespace = currentNamespace[..lastDot];
			}

			return null;
		}

		private void RecordDiscoveredType(Type type)
		{
#if UNITY_EDITOR
			var fullName = type.FullName ?? type.Name;

			if (_cache.ContainsKey(fullName)) return;

			var newRule = new TypeLogRule
			{
				typeName = fullName,
				enabled = true,
				minLogLevel = globalLogLevel,
				autoDiscovered = true
			};

			rules.Add(newRule);
			_cache[fullName] = newRule;
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}

		[Serializable]
		public class TypeLogRule
		{
			[TableColumnWidth(300)]
			[Tooltip("'System.*' | 'System.Turn.TurnService'")]
			public string typeName;

			[TableColumnWidth(80)]
			public bool enabled = true;

			[TableColumnWidth(100)]
			[EnumToggleButtons]
			public LogLevel minLogLevel = LogLevel.Debug;

			[TableColumnWidth(80)]
			[ReadOnly]
			public bool autoDiscovered;
		}

#if UNITY_EDITOR
		[Title("Editor Tools")]

		[Button("Clear Auto-Discovered Types")]
		private void ClearAutoDiscovered()
		{
			var removed = rules.RemoveAll(r => r.autoDiscovered);
			BuildCache();
			UnityEditor.EditorUtility.SetDirty(this);
			Debug.Log($"[LogSettings] Removed {removed} auto-discovered types.");
		}

		[Button("Sort Rules Alphabetically")]
		private void SortRules()
		{
			rules = rules.OrderBy(r => r.typeName).ToList();
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif
	}
}

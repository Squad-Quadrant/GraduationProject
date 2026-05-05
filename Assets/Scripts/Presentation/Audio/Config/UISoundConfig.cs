using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Audio.Config
{
	public enum EUISfx
	{
		None = 0,

		ButtonClickBattle,
		ButtonClickMenu,
		ButtonHover,
		PanelOpen,
		PanelClose,
		Error
	}

	[CreateAssetMenu(fileName = "UISoundConfig", menuName = "Game/Audio/UI Sound Config")]
	public class UISoundConfig : ScriptableObject
	{
		[Serializable]
		public struct Entry
		{
			public EUISfx kind;
			public AudioClip clip;
		}

		[SerializeField, TableList]
		private List<Entry> entries = new();

		private Dictionary<EUISfx, AudioClip> _lookup;

		public AudioClip Get(EUISfx kind)
		{
			if (_lookup == null) BuildLookup();
			return _lookup.GetValueOrDefault(kind);
		}

		private void BuildLookup()
		{
			_lookup = new Dictionary<EUISfx, AudioClip>(entries.Count);
			foreach (var entry in entries.Where(entry => entry.kind != EUISfx.None && entry.clip))
				_lookup[entry.kind] = entry.clip;
		}

		#region Editor

		[Button("Validate"), PropertyOrder(100)]
		private void Validate()
		{
			var seen = new HashSet<EUISfx>();
			var hasError = false;

			foreach (var entry in entries)
			{
				if (entry.kind == EUISfx.None)
				{
					Debug.LogWarning($"[{name}] Entry with kind 'None' is meaningless and will be ignored");
					continue;
				}
				if (!entry.clip)
				{
					Debug.LogWarning($"[{name}] '{entry.kind}': clip is null");
					continue;
				}
				if (!seen.Add(entry.kind))
				{
					Debug.LogError($"[{name}] Duplicate entry: '{entry.kind}'");
					hasError = true;
				}
			}

			var allKinds = ((EUISfx[])Enum.GetValues(typeof(EUISfx)))
				.Where(k => k != EUISfx.None);
			var missing = allKinds.Where(k => !seen.Contains(k)).ToList();
			if (missing.Count > 0)
				Debug.Log($"[{name}] Not yet configured: {string.Join(", ", missing)}");

			if (!hasError && missing.Count == 0)
				Debug.Log($"[{name}] All entries valid.");
		}

		#endregion
	}
}

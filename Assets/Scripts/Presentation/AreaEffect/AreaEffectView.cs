using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.AreaEffect;
using Data.Runtime.Events.Interaction;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using UnityEngine;

namespace Presentation.AreaEffect
{
	public class AreaEffectView : MonoBehaviour
	{
		private IEventBus _eventBus;
		private ICoordinateConverter _coordConverter;

		private readonly Dictionary<string, EffectEntry> _entries = new();

		private struct EffectEntry
		{
			public IReadOnlyList<Vector2Int> Cells;
			public GameObject PersistentVfxInstance;
		}

		public void Initialize(ServiceContainer services)
		{
			_eventBus = services.Resolve<IEventBus>();
			_coordConverter = services.Resolve<ICoordinateConverter>();

			_eventBus.Subscribe<AreaEffectRegisteredEvent>(OnRegistered);
			_eventBus.Subscribe<AreaEffectUnregisteredEvent>(OnUnregistered);

			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			if (_eventBus == null) return;   // Initialize 未调用就销毁的情况（如编辑器中断）

			_eventBus.Unsubscribe<AreaEffectRegisteredEvent>(OnRegistered);
			_eventBus.Unsubscribe<AreaEffectUnregisteredEvent>(OnUnregistered);

			foreach (var entry in _entries.Values.Where(entry => entry.PersistentVfxInstance))
				Destroy(entry.PersistentVfxInstance);
			_entries.Clear();
		}

		private void OnRegistered(AreaEffectRegisteredEvent e)
		{
			var effect = e.Effect;

			var entry = new EffectEntry
			{
				Cells = effect.Cells,
				PersistentVfxInstance = SpawnPersistentVfx(effect),
			};
			_entries[effect.Id] = entry;

			RepublishOverlay();
			this.Log($"Registered view for '{effect.Id}', cells:{effect.Cells.Count}, " +
			         $"persistentVfx:{(entry.PersistentVfxInstance ? "yes" : "none")}");
		}

		private void OnUnregistered(AreaEffectUnregisteredEvent e)
		{
			if (!_entries.TryGetValue(e.EffectId, out var entry))
			{
				this.LogWarning($"Unregister received for unknown effect '{e.EffectId}'");
				return;
			}

			if (entry.PersistentVfxInstance)
			{
				if (entry.PersistentVfxInstance.TryGetComponent<AreaEffectVfxBehavior>(out var fader))
					fader.FadeOutAndDestroy();
				else
					Destroy(entry.PersistentVfxInstance);
			}

			_entries.Remove(e.EffectId);
			RepublishOverlay();
			this.Log($"Unregistered view for '{e.EffectId}'");
		}

		private GameObject SpawnPersistentVfx(Systems.AreaEffect.AreaEffect effect)
		{
			var prefab = effect.Behavior.PersistentVfxPrefab;
			if (!prefab) return null;

			var world = _coordConverter.CellToWorld(effect.TargetCell);
			var go = Instantiate(prefab, world, Quaternion.identity, transform);
			go.name = $"AreaEffectPersistentVfx_{effect.Id}";
			return go;
		}

		private void RepublishOverlay()
		{
			if (_entries.Count == 0)
			{
				_eventBus.Publish(RangeDisplayEvent.Clear(ERangeType.AreaEffectOverlay));
				return;
			}

			var merged = new HashSet<Vector2Int>();
			foreach (var cell in _entries.Values.SelectMany(entry => entry.Cells))
				merged.Add(cell);

			_eventBus.Publish(new RangeDisplayEvent(ERangeType.AreaEffectOverlay, new List<Vector2Int>(merged)));
		}

		#region Debug

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly]
		private int DbgEffectCount => _entries.Count;

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly]
		[ListDrawerSettings(ShowFoldout = true)]
		private List<string> DbgEffectIds => new(_entries.Keys);

		#endregion
	}
}

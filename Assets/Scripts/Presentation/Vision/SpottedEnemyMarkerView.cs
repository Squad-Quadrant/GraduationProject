using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Vision;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Vision;
using UnityEngine;

namespace Presentation.Vision
{
	public class SpottedEnemyMarkerView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required] private GameObject markerPrefab;

		private IEventBus _eventBus;
		private IVisionService _visionService;
		private ICoordinateConverter _coordConverter;

		private readonly Dictionary<string, MarkerEntry> _markers = new();

		private struct MarkerEntry
		{
			public GameObject Go;
			public Vector2Int GridPosition;
		}

		public void Initialize(ServiceContainer services)
		{
			_eventBus = services.Resolve<IEventBus>();
			_visionService = services.Resolve<IVisionService>();
			_coordConverter = services.Resolve<ICoordinateConverter>();

			_eventBus.Subscribe<EnemySpottedEvent>(OnEnemySpotted);
			_eventBus.Subscribe<EnemySpotClearedEvent>(OnEnemySpotCleared);
			_eventBus.Subscribe<VisionChangedEvent>(OnVisionChanged);

			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			if (_eventBus == null) return;

			_eventBus.Unsubscribe<EnemySpottedEvent>(OnEnemySpotted);
			_eventBus.Unsubscribe<EnemySpotClearedEvent>(OnEnemySpotCleared);
			_eventBus.Unsubscribe<VisionChangedEvent>(OnVisionChanged);

			foreach (var entry in _markers.Values.Where(entry => entry.Go))
				Destroy(entry.Go);
			_markers.Clear();
		}

		private void OnEnemySpotted(EnemySpottedEvent e)
		{
			if (_markers.TryGetValue(e.UnitId, out var existing))
			{
				existing.GridPosition = e.LastKnownPosition;
				existing.Go.transform.position = _coordConverter.CellToWorld(e.LastKnownPosition);
				_markers[e.UnitId] = existing;
				RefreshVisibility(existing);
				return;
			}

			var go = Instantiate(markerPrefab, transform);
			go.name = $"SpottedMarker_{e.UnitId}";
			go.transform.position = _coordConverter.CellToWorld(e.LastKnownPosition);

			var entry = new MarkerEntry { Go = go, GridPosition = e.LastKnownPosition };
			_markers[e.UnitId] = entry;
			RefreshVisibility(entry);

			this.Log($"Created marker for '{e.UnitId}' at {e.LastKnownPosition}");
		}

		private void OnEnemySpotCleared(EnemySpotClearedEvent e)
		{
			if (!_markers.TryGetValue(e.UnitId, out var entry)) return;

			if (entry.Go) Destroy(entry.Go);
			_markers.Remove(e.UnitId);

			this.Log($"Removed marker for '{e.UnitId}'");
		}

		private void OnVisionChanged(VisionChangedEvent e)
		{
			foreach (var entry in _markers.Values)
				RefreshVisibility(entry);
		}

		private void RefreshVisibility(MarkerEntry entry)
		{
			if (!entry.Go) return;

			bool inVision = _visionService.IsCellVisible(entry.GridPosition);
			entry.Go.SetActive(!inVision);
		}

		#region Debug

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly]
		private int DbgMarkerCount => _markers.Count;

		[TitleGroup("Debug")]
		[ShowInInspector, ReadOnly]
		[ListDrawerSettings(ShowFoldout = true)]
		private List<string> DbgMarkerIds => new(_markers.Keys);

		#endregion
	}
}

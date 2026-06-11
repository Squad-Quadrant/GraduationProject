using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Map;
using Presentation.Bootstrap;
using Systems.Interfaces;
using Systems.Map;
using Systems.Map.Region;
using Systems.Map.SceneActor;
using UnityEngine;

namespace Presentation.Map.SceneActor
{
	public class SceneActorViewManager : MonoBehaviour
	{
		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		private IMapService _mapService;
		private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();

		private IRegionService _regionService;
		private IRegionService RegionService => _regionService ??= LevelContainer.Instance.Resolve<IRegionService>();

		private ICoordinateConverter _coordinateConverter;
		private ICoordinateConverter CoordinateConverter => _coordinateConverter ??= LevelContainer.Instance.Resolve<ICoordinateConverter>();

		private readonly Dictionary<uint, SceneActorView> _viewLookup = new();
		private readonly Dictionary<uint, SceneActorBase> _actorLookup = new();

		private readonly HashSet<uint> _highlightedActors = new();
		private readonly HashSet<uint> _targetBuffer = new();
		private readonly List<uint> _removeBuffer = new();

		private void OnEnable()
		{
			EventBus.Subscribe<MapViewInitEvent>(OnMapViewInit);
			EventBus.Subscribe<RegionUnlockedEvent>(OnRegionUnlocked);
			EventBus.Subscribe<SceneActorVisualChangedEvent>(OnVisualChanged);
		}

		private void OnDisable()
		{
			if (!RootContainer.Instance) return;
			EventBus.Unsubscribe<MapViewInitEvent>(OnMapViewInit);
			EventBus.Unsubscribe<RegionUnlockedEvent>(OnRegionUnlocked);
			EventBus.Unsubscribe<SceneActorVisualChangedEvent>(OnVisualChanged);
			ClearAll();
		}

		private void OnMapViewInit(MapViewInitEvent e)
		{
			ClearAll();

			foreach (var cell in e.MapData.Cells.Values)
			{
				var actor = cell.SceneActor;
				if (actor == null) continue;
				if (actor.BaseCell != cell) continue; // 只基于基准格
				CreateView(actor);
			}

			this.Log($"Created {_viewLookup.Count} SceneActor views.");
		}

		private void CreateView(SceneActorBase actor)
		{
			var go = new GameObject($"SceneActor_{actor.Uid}_{actor.DisplayName}");
			go.transform.SetParent(transform, worldPositionStays: false);

			var view = go.AddComponent<SceneActorView>();
			view.Setup(actor.Uid, actor.BaseCell.Position, actor.CurrentSlices, CoordinateConverter);

			bool visible = RegionService.IsCellUnlocked(actor.BaseCell.Position);
			view.SetAlpha(visible ? 1f : 0f);

			_viewLookup[actor.Uid] = view;
			_actorLookup[actor.Uid] = actor;
		}

		private void OnRegionUnlocked(RegionUnlockedEvent e)
		{
			foreach (var cellPos in e.Cells)
			{
				var cell = MapService.Data.GetCell(cellPos);
				var actor = cell?.SceneActor;
				if (actor == null) continue;
				if (actor.BaseCell.Position != cellPos) continue;

				if (_viewLookup.TryGetValue(actor.Uid, out var view))
					view.SetAlpha(1f);
			}
		}

		private void OnVisualChanged(SceneActorVisualChangedEvent e)
		{
			if (!_actorLookup.TryGetValue(e.Uid, out var actor))
			{
				this.LogWarning($"SceneActorVisualChangedEvent for uid {e.Uid} but no actor in lookup.");
				return;
			}
			if (!_viewLookup.TryGetValue(e.Uid, out var view))
			{
				this.LogWarning($"SceneActorVisualChangedEvent for uid {e.Uid} but no view in lookup.");
				return;
			}

			view.RefreshSlices(actor.BaseCell.Position, actor.CurrentSlices, CoordinateConverter);
		}

		public void SetHighlightedActors(IReadOnlyCollection<SceneActorBase> actors)
		{
			_targetBuffer.Clear();
			if (actors != null)
				foreach (var actor in actors)
					if (actor != null)
						_targetBuffer.Add(actor.Uid);

			_removeBuffer.Clear();
			foreach (var uid in _highlightedActors.Where(uid => !_targetBuffer.Contains(uid)))
				_removeBuffer.Add(uid);
			foreach (var uid in _removeBuffer)
			{
				if (_viewLookup.TryGetValue(uid, out var view) && view)
					view.SetHighlight(false);
				_highlightedActors.Remove(uid);
			}

			foreach (var uid in _targetBuffer.Where(uid => !_highlightedActors.Contains(uid)))
			{
				if (!_viewLookup.TryGetValue(uid, out var view) || !view) continue;

				view.SetHighlight(true);
				_highlightedActors.Add(uid);
			}
		}

		public void ClearAllHighLight()
		{
			foreach (var uid in _highlightedActors)
				if (_viewLookup.TryGetValue(uid, out var view) && view)
					view.SetHighlight(false);
			_highlightedActors.Clear();
		}

		private void ClearAll()
		{
			foreach (var view in _viewLookup.Values.Where(view => view))
				Destroy(view.gameObject);
			_viewLookup.Clear();
			_actorLookup.Clear();
		}
	}
}

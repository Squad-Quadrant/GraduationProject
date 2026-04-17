using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Presentation.Map.PathPreview
{
	[Serializable]
	public class PathTileConfig
	{
		public EPathSegmentType type;
		public TileBase tile;
	}

	[RequireComponent(typeof(Tilemap))]
	public class PathPreviewView : MonoBehaviour
	{
		[Title("Path Tile Configs")]
		[SerializeField, TableList]
		private List<PathTileConfig> pathTileConfigs = new();

		private Tilemap _tilemap;
		private Dictionary<EPathSegmentType, TileBase> _pathTileDic;

		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		private void Awake() => _tilemap = GetComponent<Tilemap>();

		private void OnEnable()
		{
			BuildPathTileDictionary();
			EventBus.Subscribe<PathPreviewEvent>(OnPathPreview);
		}

		private void OnDisable()
		{
			if (!RootContainer.Instance) return;
			EventBus.Unsubscribe<PathPreviewEvent>(OnPathPreview);
		}

		private void OnPathPreview(PathPreviewEvent e)
		{
			_tilemap.ClearAllTiles();

			if (!e.IsValid || e.Path == null || e.Path.Count < 2)
				return;

			var segments = PathTileResolver.Resolve(e.Path);
			foreach (var (pos, segmentType) in segments)
			{
				if (!_pathTileDic.TryGetValue(segmentType, out var tile) || !tile)
				{
					this.LogWarning($"Can not get tile of type: {segmentType}");
					continue;
				}
				_tilemap.SetTile((Vector3Int)pos, tile);
			}
		}

		private void BuildPathTileDictionary()
		{
			_pathTileDic = new Dictionary<EPathSegmentType, TileBase>();
			foreach (var config in pathTileConfigs)
			{
				if (_pathTileDic.ContainsKey(config.type))
				{
					this.LogWarning($"Path tile config type {config.type} dual config");
					continue;
				}
				_pathTileDic[config.type] = config.tile;
			}
		}
	}
}

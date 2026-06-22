using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Input;
using Data.Runtime.Events.Interaction;
using Data.Runtime.Events.Map;
using HighlightPlus2D;
using Presentation.Bootstrap;
using Presentation.Map.GunLine;
using Presentation.Map.PathPreview;
using Presentation.Map.SceneActor;
using Presentation.Map.Wall;
using Sirenix.OdinInspector;
using Systems.Interfaces;
using Systems.Map;
using Systems.Map.Region;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Presentation.Map
{
	public class MapView : MonoBehaviour
	{
		[Title("References")]
		[SerializeField, Required] private SpriteRenderer groundRenderer;
		[SerializeField, Required] private SceneActorViewManager sceneActorViewManager;

		[SerializeField, Required] private Tilemap cursorHoverTilemap;
		[SerializeField, Required] private Tilemap targetingTilemap;

		[Title("Cursor Hover")] [SerializeField]
		private TileBase baseSingleTile;

		[Title("GunLine")]
		[SerializeField] private GunLineView gunline;
		[SerializeField, Required] private HighlightEffect2D gunlineHighlightEffect;
		
		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		private ICoordinateConverter _coordinateConverter;

		private ICoordinateConverter CoordinateConverter =>
			_coordinateConverter ??= LevelContainer.Instance.Resolve<ICoordinateConverter>();

		private IMapService _mapService;
		private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();

		private IRegionService _regionService;
		private IRegionService RegionService => _regionService ??= LevelContainer.Instance.Resolve<IRegionService>();

		private Dictionary<EPathSegmentType, TileBase> _pathTileDic;

		private WallViewManager _wallViewManager;


		private void OnEnable()
		{
			EventBus.Subscribe<MapViewInitEvent>(InitMap);
			EventBus.Subscribe<PointerHoverEvent>(OnPointerHover);
			EventBus.Subscribe<TargetingEvent>(OnTargeting);
			EventBus.Subscribe<UpdateGunLineEvent>(UpdateGunLine);
			EventBus.Subscribe<RemoveGunLineEvent>(RemoveGunLine);
		}

		private void OnDisable()
		{
			if (!RootContainer.Instance) return;
			EventBus.Unsubscribe<MapViewInitEvent>(InitMap);
			EventBus.Unsubscribe<PointerHoverEvent>(OnPointerHover);
			EventBus.Unsubscribe<TargetingEvent>(OnTargeting);
			EventBus.Unsubscribe<UpdateGunLineEvent>(UpdateGunLine);
			EventBus.Unsubscribe<RemoveGunLineEvent>(RemoveGunLine);
		}

		private void InitMap(MapViewInitEvent e)
		{
			var mapData = e.MapData;

			if (e.GroundSprite)
			{
				groundRenderer.sprite = e.GroundSprite;
				groundRenderer.transform.position = ComputeGridOrigin(mapData.Size);
			}
			else
				this.LogWarning("No ground sprite assigned in MapConfig.");

			// 实例化墙 prefab
			var wallPrefab = e.WallVisualsPrefab;
			if (!wallPrefab)
			{
				this.LogWarning("No wall visuals prefab in MapConfig. Walls will not render.");
				return;
			}

			var instance = Instantiate(wallPrefab, transform);
			instance.name = wallPrefab.name;

			_wallViewManager = instance.GetComponent<WallViewManager>();
			if (!_wallViewManager)
			{
				this.LogWarning("Wall prefab is missing WallView component.");
				Destroy(instance);
				return;
			}

			_wallViewManager.Initialize(mapData);
			gunline.Remove();
		}

		private Vector3 ComputeGridOrigin(Vector2Int mapSize)
		{
			var (basisX, basisY) = CoordinateConverter.GetBasis();
			var center00 = CoordinateConverter.GetCenter00();

			Vector2 bottomPoint = center00 - 0.5f * basisX - 0.5f * basisY;
			Vector2 leftPoint = center00 - 0.5f * basisX + (mapSize.y - 0.5f) * basisY;
			return new Vector3(leftPoint.x, bottomPoint.y, 0f);
		}

		private void OnPointerHover(PointerHoverEvent e)
		{
			cursorHoverTilemap.ClearAllTiles();
			if (!e.CellPosition.HasValue) return;
			if (e.HoveredUnitId != null) return;
			cursorHoverTilemap.SetTile((Vector3Int)e.CellPosition.Value, baseSingleTile);
		}

		private void OnTargeting(TargetingEvent e)
		{
			targetingTilemap.ClearAllTiles();
			if (!e.TargetCell.HasValue) return;
			targetingTilemap.SetTile((Vector3Int)e.TargetCell.Value, baseSingleTile);
		}

		private void UpdateGunLine(UpdateGunLineEvent e)
		{
			Vector3 position0 = _coordinateConverter.CellToWorld(e.attacker.position);
			Vector3 position1 = _coordinateConverter.CellToWorld(e.target.position);

			Vector3 endPosition = position1;
			float lineSqrMag = (position1 - position0).sqrMagnitude;
			if (lineSqrMag > 0.0001f)
			{
				float minDistanceSqr = lineSqrMag;

				if (e.heightWalls != null)
				{
					foreach (var highWall in e.heightWalls)
					{
						var posAndIsLeft = highWall.ToPositionAndIsLeft();
						Vector2Int p1 = posAndIsLeft.Item1;
						Vector2Int p2 = posAndIsLeft.Item2 ? new Vector2Int(p1.x, p1.y + 1) : new Vector2Int(p1.x + 1, p1.y);
						Vector3 w1 = _coordinateConverter.CellToWorld(p1);
						Vector3 w2 = _coordinateConverter.CellToWorld(p2);
						Vector3 wallCenter = (w1 + w2) / 2f;
						
						float t = Vector3.Dot(wallCenter - position0, position1 - position0) / lineSqrMag;
						if (t > 0 && t < 1)
						{
							Vector3 proj = position0 + t * (position1 - position0);
							float distSqr = (proj - position0).sqrMagnitude;
							if (distSqr < minDistanceSqr)
							{
								minDistanceSqr = distSqr;
								endPosition = proj;
							}
						}
					}
				}

				if (e.sceneActors != null)
				{
					foreach (var actor in e.sceneActors)
					{
						if (actor.BaseCell == null) continue;
						Vector3 actorPos = _coordinateConverter.CellToWorld(actor.BaseCell.Position);
						float t = Vector3.Dot(actorPos - position0, position1 - position0) / lineSqrMag;
						if (t > 0 && t < 1)
						{
							Vector3 proj = position0 + t * (position1 - position0);
							float distSqr = (proj - position0).sqrMagnitude;
							if (distSqr < minDistanceSqr)
							{
								minDistanceSqr = distSqr;
								endPosition = proj;
							}
						}
					}
				}
			}

			_wallViewManager.SetHighlightedWalls(e.heightWalls);
			sceneActorViewManager.SetHighlightedActors(e.sceneActors);
			gunlineHighlightEffect.Refresh();
			
			gunline.Refresh(position0, endPosition);
		}

		private void RemoveGunLine(RemoveGunLineEvent e)
		{
			_wallViewManager.ClearAllHighLight();
			sceneActorViewManager.ClearAllHighLight();
			gunlineHighlightEffect.Refresh();

			gunline.Remove();
		}
    }
}

using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Interaction;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Map.Highlight
{
	// 高亮控制器
	public class HighlightController : MonoBehaviour
	{
		[Title("Debug")]
		[ShowInInspector, ReadOnly, LabelText("已注册的层")]
		private List<string> _registeredLayerIds = new();

		private readonly Dictionary<ERangeType, HighlightLayer> _routeMap = new();
		private readonly List<HighlightLayer> _allLayers = new();

		private IEventBus _eventBus;
		private IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		private void OnEnable()
		{
			CollectLayers();
			BuildRouteMap();

			EventBus.Subscribe<RangeDisplayEvent>(OnRangeDisplay);
			EventBus.Subscribe<UnitSelectedEvent>(OnUnitSelected);
			EventBus.Subscribe<UnitDeselectedEvent>(OnUnitDeselected);
		}

		private void OnDisable()
		{
			if (!RootContainer.Instance) return;
			EventBus.Unsubscribe<RangeDisplayEvent>(OnRangeDisplay);
			EventBus.Unsubscribe<UnitSelectedEvent>(OnUnitSelected);
			EventBus.Unsubscribe<UnitDeselectedEvent>(OnUnitDeselected);
		}

		private void CollectLayers()
		{
			_allLayers.Clear();
			_allLayers.AddRange(GetComponentsInChildren<HighlightLayer>(includeInactive: true));

			_registeredLayerIds.Clear();
			foreach (var layer in _allLayers)
				_registeredLayerIds.Add(layer.LayerId);
		}

		private void BuildRouteMap()
		{
			_routeMap.Clear();
			foreach (var layer in _allLayers)
			{
				foreach (var rangeType in layer.ServingRangeTypes)
				{
					if (_routeMap.TryGetValue(rangeType, out var existing))
					{
						this.LogError(
							$"路由冲突: RangeType={rangeType} 同时被 '{existing.LayerId}' 和 '{layer.LayerId}' 声明。" +
							$"保留 '{existing.LayerId}'。");
						continue;
					}
					_routeMap[rangeType] = layer;
				}
			}
			this.Log($"路由表构建完成：{_routeMap.Count} 条映射，覆盖 {_allLayers.Count} 个 Layer");
		}

		private void OnRangeDisplay(RangeDisplayEvent e)
		{
			if (!_routeMap.TryGetValue(e.RangeType, out var layer)) return;

			if (e.RangeType == ERangeType.Movement)
				layer.SetMovement(e.Cells, e.CellCosts);
			else
				layer.Set(e.Cells);
		}

		private void OnUnitSelected(UnitSelectedEvent e)
		{
			if (!_routeMap.TryGetValue(ERangeType.Selection, out var layer)) return;
			layer.Set(new[] { e.Position });
		}

		private void OnUnitDeselected(UnitDeselectedEvent e)
		{
			if (!_routeMap.TryGetValue(ERangeType.Selection, out var layer)) return;
			layer.Clear();
		}

	}
}

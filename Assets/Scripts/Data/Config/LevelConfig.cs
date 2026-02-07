using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.Config
{
	/// <summary>
	/// Defines a single unit's initial placement in the level.
	/// </summary>
	[Serializable]
	public class UnitPlacement
	{
		[HorizontalGroup("Main")]
		[LabelText("单位ID"), LabelWidth(60)]
		[InfoBox("该单位实例的运行时唯一标识符", InfoMessageType.None)] // todo: 考虑自动分配局内id
		public string unitId;

		[HorizontalGroup("Main")]
		[LabelText("单位配置"), LabelWidth(60)]
		[Required]
		public UnitConfig unitConfig;

		[HorizontalGroup("Position")]
		[LabelText("初始位置"), LabelWidth(60)]
		public Vector2Int startPosition;
	}

	[CreateAssetMenu(fileName = "NewLevelConfig", menuName = "Game/Level Config")]
	public class LevelConfig : ScriptableObject
	{
		#region Basic Info

		[Title("基础信息", bold: true)]
		[LabelText("关卡ID")]
		[InfoBox("关卡的唯一标识符")]
		public string levelId = "level_001";

		[Space]
		[LabelText("关卡名称")]
		public string levelName = "New Level";

		[Space]
		[LabelText("关卡描述")]
		[TextArea(2, 4)]
		public string description = "This is a new level.";

		#endregion

		#region Map Reference

		[Title("地图配置", bold: true)]
		[LabelText("地图配置")]
		[Required]
		[InfoBox("关卡所使用的地图配置")]
		public MapConfig mapConfig;

		#endregion

		#region Unit Placements

		[Title("单位配置", bold: true)]
		[LabelText("初始单位")]
		[InfoBox("定义关卡开始时生成的单位及其位置", InfoMessageType.None)]
		[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
		public List<UnitPlacement> unitPlacements = new();

		#endregion

		#region Validation

		[Button("验证配置", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
		public void ValidateConfig()
		{
			bool isValid = true;

			// Check level ID
			if (string.IsNullOrEmpty(levelId))
			{
				Debug.LogError($"[LevelConfig] Level ID cannot be empty!");
				isValid = false;
			}

			// Check map config
			if (!mapConfig)
			{
				Debug.LogError($"[LevelConfig] Map configuration is missing!");
				isValid = false;
			}

			// Check unit placements
			if (unitPlacements.Count == 0)
				Debug.LogWarning($"[LevelConfig] No units defined for this level!");

			// Check for duplicate unit IDs
			var unitIds = new HashSet<string>();
			foreach (var placement in unitPlacements)
			{
				if (string.IsNullOrEmpty(placement.unitId))
				{
					Debug.LogError($"[LevelConfig] Unit placement has empty ID!");
					isValid = false;
					continue;
				}

				if (!unitIds.Add(placement.unitId))
				{
					Debug.LogError($"[LevelConfig] Duplicate unit ID: {placement.unitId}");
					isValid = false;
				}

				if (!placement.unitConfig)
				{
					Debug.LogError($"[LevelConfig] Unit '{placement.unitId}' has no config assigned!");
					isValid = false;
				}
			}

			if (isValid)
				Debug.Log($"✓ [{levelName}] Configuration is valid!");
			else
				Debug.LogError($"✗ [{levelName}] Configuration has errors, please check above messages.");
		}

		#endregion

		#region Editor Display

		[ShowInInspector, DisplayAsString, HideLabel]
		[PropertyOrder(-1)]
		[PropertySpace(SpaceAfter = 10)]
		private string EditorTitle => $"关卡配置: {levelName} ({levelId})";

		#endregion
	}
}

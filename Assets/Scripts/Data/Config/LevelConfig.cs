using System;
using System.Collections.Generic;
using PurpleFlowerCore;
using Sirenix.OdinInspector;
using Systems.Map.Config;
using Systems.Unit;
using UnityEngine;

namespace Data.Config
{
	// LevelConfig中的单位插槽
	[Serializable]
	public class UnitPlacement
	{
		[HorizontalGroup("Main")]
		[LabelText("单位ID"), LabelWidth(60)]
		[InfoBox("该单位实例的运行时唯一标识符", InfoMessageType.None)]
		public string unitId;

		[HorizontalGroup("Main")]
		[LabelText("单位配置"), LabelWidth(60)]
		[Required]
		public UnitConfig unitConfig;

		[HorizontalGroup("Position")]
		[LabelText("初始位置"), LabelWidth(60)]
		public Vector2Int startPosition;

		[LabelText("初始装备配置")]
		[InfoBox("玩家单位在局外配装后会覆盖此值；敌人/NPC 直接使用此值", InfoMessageType.None)]
		public Loadout initialLoadout = new();

		[LabelText("巡逻路线"), ShowIf("CurrentUnitFaction", EUnitFaction.Enemy)]
		[InfoBox("仅 AIArchetype.idleBehavior=Patrol 时使用；按顺序循环。", InfoMessageType.None)]
		public List<Vector2Int> patrolWaypoints = new();

		[LabelText("激活组 ID"), ShowIf("CurrentUnitFaction", EUnitFaction.Enemy)]
		[InfoBox("0 = 独立激活（不与其他敌人连锁）。同组任一成员被激活时，全组激活。", InfoMessageType.None)]
		public int activationGroupId = 0;

		private EUnitFaction CurrentUnitFaction => unitConfig.faction;
	}

    [Configurable("Level")]
	[CreateAssetMenu(fileName = "NewLevelConfig", menuName = "Game/Level Config")]
	public class LevelConfig : ScriptableObject
	{
		[Title("基础信息", bold: true)]
		[LabelText("关卡ID")]
		[InfoBox("关卡的唯一标识符")]
		public string levelId = "level_001";

		[LabelText("关卡名称")]
		public string levelName = "New Level";

		[LabelText("关卡描述")]
		[TextArea(2, 4)]
		public string description = "This is a new level.";


		[Title("地图配置", bold: true)]
		[LabelText("地图配置")]
		[Required]
		[InfoBox("关卡所使用的地图配置")]
		public MapConfig mapConfig;


		[Title("单位配置", bold: true)]
		[LabelText("初始单位")]
		[InfoBox("定义关卡开始时生成的单位及其位置", InfoMessageType.None)]
		[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
		public List<UnitPlacement> unitPlacements = new();

		[Title("音效", bold: true)]
		[LabelText("战斗 BGM")] public AudioClip battleBgm;
		[LabelText("BGM 淡入时长(秒)")] [Range(0f, 3f)] public float bgmFadeIn = 1f;


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

		#region Editor Display

		[ShowInInspector, DisplayAsString, HideLabel]
		[PropertyOrder(-1)]
		[PropertySpace(SpaceAfter = 10)]
		private string EditorTitle => $"关卡配置: {levelName} ({levelId})";

		#endregion
	}
}

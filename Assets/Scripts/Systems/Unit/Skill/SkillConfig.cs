using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Unit.Skill
{
	[CreateAssetMenu(fileName = "SkillConfig", menuName = "Game/Unit/Skill Config")]
	public class SkillConfig : ScriptableObject
	{
		[Title("基础信息")]
		[LabelText("ID")]
		public string id = "skill_001";

		[LabelText("名称")]
		public string skillName = "New Skill";

		[LabelText("描述"), TextArea(2, 4)]
		public string description = "";

		[LabelText("图标"), PreviewField(60, ObjectFieldAlignment.Left)]
		public Sprite icon;

		[Title("技能类型")]
		[LabelText("技能种类")]
		public ESkillKind kind = ESkillKind.InstantHeal;

		[Title("通用消耗")]
		[LabelText("AP 消耗"), MinValue(0)]
		public int apCost = 1;

		[LabelText("冷却回合数"), MinValue(0)]
		[InfoBox("0 = 无冷却（只受 AP 限制）。N = 使用后过 N 个自己的回合才能再用。")]
		public int cooldown = 2;

		[Title("InstantHeal 参数"), ShowIf(nameof(kind), ESkillKind.InstantHeal)]
		[LabelText("治疗量"), MinValue(1)]
		public int healAmount = 30;

		[Title("ScoutEye 参数"), ShowIf(nameof(kind), ESkillKind.ScoutEye)]
		[LabelText("施放最大距离（曼哈顿）"), MinValue(1)]
		public int range = 5;

		[ShowIf(nameof(kind), ESkillKind.ScoutEye)]
		[LabelText("覆盖格相对偏移")]
		[Tooltip("相对 TargetCell（落点）的偏移，决定 AreaEffect 的覆盖格形状。必须包含 (0,0) 才能包括落点本身。")]
		public Vector2Int[] coverageOffsets = { Vector2Int.zero };

		[ShowIf(nameof(kind), ESkillKind.ScoutEye)]
		[LabelText("透视半径"), MinValue(1)]
		[Tooltip("侦察眼生效后，以 TargetCell 为中心产生的视野半径。")]
		public int visionReach = 5;

		[ShowIf(nameof(kind), ESkillKind.ScoutEye)]
		[LabelText("持续回合数"), MinValue(1)]
		public int persistTurns = 2;

		[Title("ScoutEye 视觉特效"), ShowIf(nameof(kind), ESkillKind.ScoutEye)]
		[LabelText("瞬时特效")]
		public GameObject oneShotVfxPrefab;

		[ShowIf(nameof(kind), ESkillKind.ScoutEye)]
		[LabelText("持续区域特效")]
		public GameObject persistentVfxPrefab;
	}
}

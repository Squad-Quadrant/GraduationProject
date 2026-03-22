using Presentation.Unit;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Systems.Unit
{
	[CreateAssetMenu(fileName = "NewUnitConfig", menuName = "Game/Unit Config")]
	public class UnitConfig : ScriptableObject
	{
		[TitleGroup("基础信息")]
		public string configId = "unit_001";
		public string unitName = "New Unit";
		[TextArea(2, 4)]
		public string description = "This is a new unit.";

		[TitleGroup("视觉表现")]
		[PreviewField(80, ObjectFieldAlignment.Left)]
		public Sprite icon;
		[Required] public UnitAnimationConfig animationConfig;
		[Required] public SkeletonDataAsset skeletonDataAsset;
		public string frontBodySkin = "pcb1 front";
		public string backBodySkin = "pcb1 back";
		public string defaultWeaponSkin = "";

		[TitleGroup("单位属性")]
		public int maxHp = 100;
		public int speed = 10;
		public int moveRange = 2; // 每消耗一个行动点可以移动的格子数量
		public int actionPoints = 2; // 每回合可用的行动点数
        public int visionRange = 4;
		public EUnitFaction faction = EUnitFaction.Neutral;
        public int defense = 100;
        public float defenseRate = 0f;
        public int quickness = 100; // 用于大回合行动顺序的计算
        

		#region Validation

		[Button("验证配置", ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
		public void ValidateConfig()
		{
			bool isValid = true;

			if (string.IsNullOrEmpty(configId))
			{
				Debug.LogError($"[UnitConfig] [{unitName}] Config ID cannot be empty!");
				isValid = false;
			}

			if (!icon) Debug.LogWarning($"[UnitConfig] [{unitName}] No icon assigned!");

			if (!animationConfig)
			{
				Debug.LogError($"[UnitConfig] [{unitName}] No animation config assigned!");
				isValid = false;
			}

			if (!skeletonDataAsset)
			{
				Debug.LogError($"[UnitConfig] [{unitName}] No skeleton data asset assigned!");
				isValid = false;
			}

			if (string.IsNullOrEmpty(frontBodySkin) || string.IsNullOrEmpty(backBodySkin))
			{
				Debug.LogError($"[UnitConfig] [{unitName}] Front and back body skins must be assigned!");
				isValid = false;
			}

			if (isValid)
				Debug.Log($"✓ [{unitName}] Configuration is valid!");
			else
				Debug.LogError($"✗ [{unitName}] Configuration has errors, please check above messages.");
		}

		#endregion


		#region Editor Display

		[ShowInInspector, DisplayAsString, HideLabel]
		[PropertyOrder(-1)]
		[PropertySpace(SpaceAfter = 10)]
		private string EditorTitle => $"单位配置: {unitName} ({configId})";

		#endregion
	}
}

using System;
using Sirenix.OdinInspector;
using Systems.Unit;
using UnityEngine;

namespace Data.Config
{
	[CreateAssetMenu(fileName = "NewUnitConfig", menuName = "Game/Unit Config")]
	public class UnitConfig : ScriptableObject
	{
		#region Basic Info

		[Title("基础信息", bold: true)]
		[LabelText("配置ID")]
		[InfoBox("单位的唯一标识符", InfoMessageType.None)]
		public string configId = "unit_001";

		[Space]
		[LabelText("单位名称")]
		[InfoBox("单位的显示名称", InfoMessageType.None)]
		public string unitName = "New Unit";

		[Space]
		[LabelText("单位描述")]
		[TextArea(2, 4)]
		public string description = "This is a new unit.";

		#endregion


		#region Visuals

		[Title("视觉表现", bold: true)]
		[LabelText("单位图标")]
		[PreviewField(80, ObjectFieldAlignment.Left)]
		public Sprite icon;

		[Space]
		[LabelText("单位预制体")]
		public GameObject prefab; // todo: 考虑单一的单位预制体,在生成单位时为该预制体做装饰，而不是为每个单位配置一个预制体

		#endregion


		#region Stats

		[Title("单位属性", bold: true)]
		[LabelText("属性配置")]
		public UnitStats stats = new()
		{
			maxHp = 100,
			speed = 10,
			moveRange = 2,
			actionPoints = 2,
			faction = UnitFaction.Neutral
		};

		#endregion


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

			if (!prefab)
			{
				Debug.LogWarning($"[UnitConfig] [{unitName}] No prefab assigned!");
			}

			if (!icon)
			{
				Debug.LogWarning($"[UnitConfig] [{unitName}] No icon assigned!");
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

using System;
using Sirenix.OdinInspector;
using Systems.Unit;
using UnityEngine;

namespace Data.Config
{
	[CreateAssetMenu(fileName = "NewUnitConfig", menuName = "Game/Unit Config")]
	public class UnitConfig : ScriptableObject
	{
		#region basic info

		[Title("Basic Info", bold: true)]
		[LabelText("配置ID")]
		[InfoBox("单位的唯一标识符", InfoMessageType.None)]
		public string configId = "unit_001";

		[Space]
		[LabelText("单位名称")]
		[InfoBox("单位的显示名称", InfoMessageType.None)]
		public string unitName = "New Unit";

		[Space]
		[LabelText("描述")]
		[TextArea(2, 4)]
		public string description = "This is a new unit.";

		#endregion


		#region visuals

		[Title("Visuals", bold: true)]
		[LabelText("单位图标")]
		[PreviewField(80, ObjectFieldAlignment.Left)]
		public Sprite icon;

		[Space]
		[LabelText("预制体")]
		public GameObject prefab;

		#endregion


		#region stats

		[Title("Stats", bold: true)]
		public UnitStats stats = new()
		{
			maxHp = 100,
			speed = 10,
			moveRange = 2,
			actionPoints = 2
		};

		#endregion


		#region tools

		[Button(ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
		public void ValidateConfig()
		{
			bool isValid = true;

			if (string.IsNullOrEmpty(configId))
			{
				Debug.LogError($"[UnitConfig] [{unitName}] 配置ID不能为空！");
				isValid = false;
			}

			if (isValid)
				Debug.Log($"✓ [{unitName}] 配置验证通过！");
			else
				Debug.LogError($"✗ [{unitName}] 配置验证失败，请检查错误信息。");
		}

		[ShowInInspector, DisplayAsString, HideLabel]
		[PropertyOrder(-1)]
		[PropertySpace(SpaceAfter = 10)]
		private string EditorTitle => $"单位配置: {unitName} ({configId})";

		#endregion
	}
}

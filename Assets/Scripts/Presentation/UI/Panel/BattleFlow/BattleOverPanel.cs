using System;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.BattleFlow
{
	public struct BattleOverPanelData
	{
		public bool IsVictory;
		public Action OnRestart;
		public Action OnReturnToMenu;
	}

	public class BattleOverPanel : UIPanel, IInitializable<BattleOverPanelData>
	{
		[Title("Display")]
		[SerializeField, Required, ChildGameObjectsOnly]
		private TextMeshProUGUI titleText;

		[SerializeField, Tooltip("胜利时显示的标题文字")]
		private string victoryText = "行动胜利！";

		[SerializeField]
		private Color victoryColor = Color.green;

		[SerializeField, Tooltip("失败时显示的标题文字")]
		private string defeatText = "行动失败";

		[SerializeField]
		private Color defeatColor = Color.red;

		[Title("Buttons")]
		[SerializeField, Required, ChildGameObjectsOnly]
		private Button restartButton;

		[SerializeField, Required, ChildGameObjectsOnly]
		private Button returnToMenuButton;

		public void DataInitialize(BattleOverPanelData data)
		{
			titleText.text = data.IsVictory ? victoryText : defeatText;
			titleText.color = data.IsVictory ? victoryColor : defeatColor;

			restartButton.onClick.RemoveAllListeners();
			restartButton.onClick.AddListener(() => data.OnRestart?.Invoke());

			returnToMenuButton.onClick.RemoveAllListeners();
			returnToMenuButton.onClick.AddListener(() => data.OnReturnToMenu?.Invoke());
		}
	}
}

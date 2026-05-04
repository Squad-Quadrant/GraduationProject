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
		private Image resultImage;

		[SerializeField, AssetsOnly]
		private Sprite winSprite;

		[SerializeField, AssetsOnly]
		private Sprite loseSprite;

		[SerializeField, Required, ChildGameObjectsOnly]
		private Image descImage;

		[SerializeField, AssetsOnly]
		private Sprite winDescSprite;

		[SerializeField, AssetsOnly]
		private Sprite loseDescSprite;

		[Title("Buttons")]
		[SerializeField, Required, ChildGameObjectsOnly]
		private Button restartButton;

		[SerializeField, Required, ChildGameObjectsOnly]
		private Button returnToMenuButton;

		public void DataInitialize(BattleOverPanelData data)
		{
			resultImage.sprite = data.IsVictory ? winSprite : loseSprite;
			descImage.sprite = data.IsVictory ? winDescSprite : loseDescSprite;

			restartButton.onClick.RemoveAllListeners();
			restartButton.onClick.AddListener(() => data.OnRestart?.Invoke());

			returnToMenuButton.onClick.RemoveAllListeners();
			returnToMenuButton.onClick.AddListener(() => data.OnReturnToMenu?.Invoke());
		}
	}
}

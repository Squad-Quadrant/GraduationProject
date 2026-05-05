using System;
using System.Collections.Generic;
using System.Linq;
using Data.Config;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.Menu
{
	public class LevelSelectPanel : UIPanel, IInitializable<LevelSelectPanelData>
	{
		[Serializable]
		private class Entry
		{
			[Required, HorizontalGroup("main", Width = 0.5f)]
			public LevelConfig level;

			[Required, HorizontalGroup("main")]
			public Button button;
		}

		[Title("Level Entries")]
		[SerializeField]
		[ListDrawerSettings(ShowFoldout = true)]
		[InfoBox("每个条目 = 一个关卡按钮。button 的点击会触发 OnLevelSelected(level)")]
		private List<Entry> entries = new();

		[Title("Buttons")]
		[SerializeField, Required, ChildGameObjectsOnly] private Button backButton;
		[SerializeField, Required, ChildGameObjectsOnly] private Button leftButton;
		[SerializeField, Required, ChildGameObjectsOnly] private Button rightButton;

		private LevelSelectPanelData _data;

		public void DataInitialize(LevelSelectPanelData data) => _data = data;

		protected override void OnOpen()
		{
			foreach (var entry in entries)
			{
				if (!entry?.level || !entry.button) continue;

				var captured = entry.level;
				entry.button.onClick.AddListener(() => _data.OnLevelSelected?.Invoke(captured));
			}

			backButton.onClick.AddListener(() => _data.OnBack?.Invoke());
		}

		protected override void OnClose()
		{
			foreach (var entry in entries.Where(entry => entry?.level && entry.button)) entry.button.onClick.RemoveAllListeners();
			backButton.onClick.RemoveAllListeners();
		}
	}

	public struct LevelSelectPanelData
	{
		public Action<LevelConfig> OnLevelSelected;
		public Action OnBack;
	}
}

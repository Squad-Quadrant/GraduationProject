using System;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.Menu
{
	public class CrewPanel : UIPanel, IInitializable<CrewPanelData>
	{
		[Title("Buttons")]
		[SerializeField, Required, ChildGameObjectsOnly] private Button backButton;

		private CrewPanelData _data;

		public void DataInitialize(CrewPanelData data) => _data = data;

		protected override void OnOpen() => backButton.onClick.AddListener(() => _data.OnBack?.Invoke());

		protected override void OnClose() => backButton.onClick.RemoveAllListeners();
	}

	public struct CrewPanelData
	{
		public Action OnBack;
	}
}

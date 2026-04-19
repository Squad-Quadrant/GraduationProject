using System;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel.Menu
{
	public class MainMenuPanel : UIPanel, IInitializable<MainMenuPanelData>
	{
		[Title("Buttons")]
		[SerializeField, Required, ChildGameObjectsOnly] private Button startButton;
		[SerializeField, Required, ChildGameObjectsOnly] private Button quitButton;

		public void DataInitialize(MainMenuPanelData data)
		{
			startButton.onClick.RemoveAllListeners();
			startButton.onClick.AddListener(() => data.OnStart?.Invoke());

			quitButton.onClick.RemoveAllListeners();
			quitButton.onClick.AddListener(() => data.OnQuit?.Invoke());
		}
	}

	public struct MainMenuPanelData
	{
		public Action OnStart;
		public Action OnQuit;
	}
}

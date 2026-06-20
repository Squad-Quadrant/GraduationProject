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
		[SerializeField, Required, ChildGameObjectsOnly] private Button crewButton;

		private MainMenuPanelData _data;

		public void DataInitialize(MainMenuPanelData data) => _data = data;

		protected override void OnOpen()
		{
			startButton.onClick.AddListener(OnStartButtonClicked);
			quitButton.onClick.AddListener(OnQuitButtonClicked);
			crewButton.onClick.AddListener(OnCrewButtonClicked);
		}

		protected override void OnClose()
		{
			startButton.onClick.RemoveListener(OnStartButtonClicked);
			quitButton.onClick.RemoveListener(OnQuitButtonClicked);
			crewButton.onClick.RemoveListener(OnCrewButtonClicked);
		}

		private void OnStartButtonClicked() => _data.OnStart?.Invoke();
		private void OnQuitButtonClicked() => _data.OnQuit?.Invoke();
		private void OnCrewButtonClicked() => _data.OnCrew?.Invoke();
	}

	public struct MainMenuPanelData
	{
		public Action OnStart;
		public Action OnQuit;
		public Action OnCrew;
	}
}

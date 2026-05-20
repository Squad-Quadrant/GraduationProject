using System;
using Presentation.Audio;
using Presentation.CameraControl;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Systems.Time;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
	public struct SettingPanelData
	{
		public AudioService AudioService;
		public CameraController CameraController;
		public ITimeService TimeService;
		public Action OnReturnToMenu;
		public Action OnBack;
	}

	public class BattleSettingPanel : UIPanel, IInitializable<SettingPanelData>
	{
		[SerializeField, ChildGameObjectsOnly, Required] private Slider masterVolumeSlider;
		[SerializeField, ChildGameObjectsOnly, Required] private Slider bgmVolumeSlider;
		[SerializeField, ChildGameObjectsOnly, Required] private Slider sfxVolumeSlider;
		[SerializeField, ChildGameObjectsOnly, Required] private Button returnToMenuBtn;
		[SerializeField, ChildGameObjectsOnly, Required] private Button backBtn;

		private SettingPanelData _data;

		public void DataInitialize(SettingPanelData data)
		{
			_data = data;

			masterVolumeSlider.value = data.AudioService.GetVolume(EVolumeChannel.Master);
			bgmVolumeSlider.value = data.AudioService.GetVolume(EVolumeChannel.BGM);
			sfxVolumeSlider.value = data.AudioService.GetVolume(EVolumeChannel.SFX);

			masterVolumeSlider.onValueChanged.AddListener(value => data.AudioService.SetVolume(EVolumeChannel.Master, value));
			bgmVolumeSlider.onValueChanged.AddListener(value => data.AudioService.SetVolume(EVolumeChannel.BGM, value));
			sfxVolumeSlider.onValueChanged.AddListener(value => data.AudioService.SetVolume(EVolumeChannel.SFX, value));

			returnToMenuBtn.onClick.AddListener(() => data.OnReturnToMenu?.Invoke());
			backBtn.onClick.AddListener(() => data.OnBack?.Invoke());
		}

		protected override void OnOpen()
		{
			_data.CameraController.SetEnabled(false);
			_data.TimeService.SetTimeScale(0);
		}

		protected override void OnClose()
		{
			_data.CameraController.SetEnabled(true);
			_data.TimeService.ResetTimeScale();
		}
	}
}

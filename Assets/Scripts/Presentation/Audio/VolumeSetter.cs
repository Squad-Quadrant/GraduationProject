using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Audio
{
	[CreateAssetMenu(fileName = "VolumeSetter", menuName = "Game/Audio/Volume Setter")]
	public class VolumeSetter : ScriptableObject
	{
		[OnValueChanged("SetGlobalVolume")] [SerializeField, Range(0, 1)] private float globalVolume = 0.5f;
		[OnValueChanged("SetBgmVolume")] [SerializeField, Range(0, 1)] private float bgmVolume = 0.5f;
		[OnValueChanged("SetSfxVolume")] [SerializeField, Range(0, 1)] private float sfxVolume = 0.5f;

		private void SetGlobalVolume() => SetVolume(EVolumeChannel.Master, globalVolume);
		private void SetBgmVolume() => SetVolume(EVolumeChannel.BGM, bgmVolume);
		private void SetSfxVolume() => SetVolume(EVolumeChannel.SFX, sfxVolume);

		[Button]
		private void SyncVolume()
		{
			if (!TryGetAudioService(out var service))
			{
				Debug.LogWarning("[Volume Setter] Audio service not found");
				return;
			}
			globalVolume = service.GetVolume(EVolumeChannel.Master);
			bgmVolume = service.GetVolume(EVolumeChannel.BGM);
			sfxVolume = service.GetVolume(EVolumeChannel.SFX);
		}

		private static void SetVolume(EVolumeChannel channel, float value)
		{
			if (!TryGetAudioService(out var service))
			{
				Debug.LogWarning("[Volume Setter] Audio service not found");
				return;
			}
			service.SetVolume(channel, value);
			Debug.Log($"[Volume Setter] {channel.ToString()} -> {value}");
		}

		private static bool TryGetAudioService(out AudioService audioService)
		{
			audioService = null;
			if (!RootContainer.Instance) return false;
			audioService = RootContainer.Instance.Resolve<AudioService>();
			return audioService;
		}
	}
}

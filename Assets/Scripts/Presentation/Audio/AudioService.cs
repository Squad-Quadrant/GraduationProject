using System;
using Core.Log;
using DG.Tweening;
using Presentation.Audio.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace Presentation.Audio
{
	// 全局音频播放类
	public class AudioService : MonoBehaviour
	{
		[Title("Configuration")]
		[SerializeField, Required] private AudioMixer mixer;
		[SerializeField, Required] private UISoundConfig uiSoundConfig;

		[Title("Sources")]
		[SerializeField, Required, ChildGameObjectsOnly] private AudioSource bgmSource;
		[SerializeField, Required, ChildGameObjectsOnly] private AudioSource sfxSource;

		[Title("Settings")]
		[SerializeField, Range(0f, 1f), Tooltip("首次启动时（PlayerPrefs 无记录）使用的默认音量")]
		private float defaultVolume = 0.8f;

		private Tween _bgmTween;
		private bool _initialized;

		private const string PrefKeyPrefix = "Audio.Volume.";
		private const float MinDb = -80f;

		public void Initialize()
		{
			if (_initialized) return;
			_initialized = true;

			LoadAndApplyAllVolumes();

			if (!uiSoundConfig)
				this.LogWarning("uiSoundConfig未配置");

			this.Log("Initialized");
		}

		private void OnDestroy()
		{
			_bgmTween?.Kill();
			this.Log("Destroyed");
		}

		public void PlayBGM(AudioClip clip, float fade = 0f)
		{
			if (!clip)
			{
				this.LogWarning("PlayBGM called with null clip");
				return;
			}

			if (bgmSource.clip == clip && bgmSource.isPlaying) return; // 同 clip 不重播

			_bgmTween?.Kill();

			if (!bgmSource.isPlaying)
			{
				bgmSource.clip = clip;
				bgmSource.volume = fade > 0f ? 0f : 1f;
				bgmSource.Play();
				if (fade > 0f)
					_bgmTween = bgmSource.DOFade(1f, fade);
				return;
			}

			if (fade <= 0f)
			{
				bgmSource.Stop();
				bgmSource.clip = clip;
				bgmSource.volume = 1f;
				bgmSource.Play();
				return;
			}

			var halfFade = fade * 0.5f;
			_bgmTween = bgmSource
				.DOFade(0f, halfFade)
				.OnComplete(() =>
				{
					bgmSource.clip = clip;
					bgmSource.Play();
					_bgmTween = bgmSource.DOFade(1f, halfFade);
				});
		}

		public void StopBGM(float fade = 0f)
		{
			_bgmTween?.Kill();

			if (!bgmSource.isPlaying) return;

			if (fade <= 0f)
			{
				bgmSource.Stop();
				return;
			}

			_bgmTween = bgmSource
				.DOFade(0f, fade)
				.OnComplete(() => bgmSource.Stop());
		}

		public void PlayUISfx(EUISfx kind)
		{
			if (kind == EUISfx.None || !uiSoundConfig) return;
			var clip = uiSoundConfig.Get(kind);
			if (clip) PlaySFX(clip);
		}

		public void PlaySFX(AudioClip clip, float volumeScale = 1f)
		{
			if (!clip) return;
			sfxSource.PlayOneShot(clip, volumeScale);
		}

		public void SetVolume(EVolumeChannel channel, float v01)
		{
			v01 = Mathf.Clamp01(v01);
			ApplyVolumeToMixer(channel, v01);
			PlayerPrefs.SetFloat(PrefKey(channel), v01);
		}

		public float GetVolume(EVolumeChannel channel)
		{
			var paramName = GetMixerParamName(channel);
			if (mixer.GetFloat(paramName, out var db))
				return DbToLinear(db);

			this.LogError($"Mixer parameter '{paramName}' not exposed. Did you set it in the AudioMixer asset?");
			return 0f;
		}

		private void LoadAndApplyAllVolumes()
		{
			foreach (EVolumeChannel ch in Enum.GetValues(typeof(EVolumeChannel)))
			{
				var key = PrefKey(ch);
				var v01 = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : defaultVolume;
				ApplyVolumeToMixer(ch, v01);
			}
		}

		private void ApplyVolumeToMixer(EVolumeChannel channel, float v01)
		{
			var paramName = GetMixerParamName(channel);
			var db = LinearToDb(v01);
			if (!mixer.SetFloat(paramName, db))
				this.LogError($"Mixer parameter '{paramName}' not exposed. Did you set it in the AudioMixer asset?");
		}

		private static string GetMixerParamName(EVolumeChannel channel) => channel.ToString();

		private static string PrefKey(EVolumeChannel channel) => PrefKeyPrefix + channel;

		private static float LinearToDb(float linear) => linear > 0.0001f ? 20f * Mathf.Log10(linear) : MinDb;

		private static float DbToLinear(float db) => Mathf.Pow(10f, db / 20f);
	}
}

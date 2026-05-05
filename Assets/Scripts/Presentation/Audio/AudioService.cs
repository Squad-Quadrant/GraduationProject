using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		[InfoBox("首次启动时（PlayerPrefs 无记录）使用的默认音量")]
		[SerializeField, Range(0f, 1f)] private float defaultGlobalVolume = 0.5f;
		[SerializeField, Range(0f, 1f)] private float defaultBgmVolume = 0.3f;
		[SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.5f;



		private Tween _bgmTween;
		private bool _initialized;

		private readonly Stack<AudioSource> _freeSources = new();
		private readonly Dictionary<int, AudioSource> _activeLoops = new();
		private int _nextHandleId = 1;

		private const string PrefKeyPrefix = "Audio.Volume.";
		private const float MinDb = -80f;

		public void Initialize()
		{
			if (_initialized) return;
			_initialized = true;

			if (!uiSoundConfig)
				this.LogWarning("uiSoundConfig未配置");

			StartCoroutine(ApplyInitialVolumesNextFrame());

			this.Log("Initialized");
		}

		private IEnumerator ApplyInitialVolumesNextFrame()
		{
			yield return null; // 等到下一帧，AudioMixer需要一帧初始化
			LoadAndApplyAllVolumes();
		}

		private void OnDestroy()
		{
			_bgmTween?.Kill();

			foreach (var src in _activeLoops.Values.Where(src => src))
			{
				DOTween.Kill(src);
				src.Stop();
			}
			_activeLoops.Clear();

			this.Log("Destroyed");
		}

		#region BGM

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

		#endregion

		#region SFX

		public void PlayUISfx(EUISfx kind)
		{
			if (kind == EUISfx.None || !uiSoundConfig) return;
			var clip = uiSoundConfig.Get(kind);
			if (clip) PlaySfx(clip);
		}

		public void PlaySfx(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
		{
			if (!clip)
			{
				this.LogWarning("PlaySfx called with null clip");
				return;
			}

			if (pitch <= 0f)
			{
				this.LogWarning($"PlaySFX: pitch must be > 0, got {pitch}. Falling back to 1.");
				pitch = 1f;
			}

			if (Mathf.Approximately(pitch, 1f))
			{
				sfxSource.PlayOneShot(clip, volumeScale);
				return;
			}

			var src = AcquireSource();
			src.clip = clip;
			src.volume = volumeScale;
			src.pitch = pitch;
			src.loop = false;
			src.Play();

			StartCoroutine(ReleaseAfter(src, clip.length / pitch));
		}

		public SfxHandle PlayLoop(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
		{
			if (!clip)
			{
				this.LogWarning("PlayLoop called with null clip");
				return SfxHandle.Invalid;
			}

			if (pitch <= 0f)
			{
				this.LogWarning($"PlayLoop: pitch must be > 0, got {pitch}. Using 1.");
				pitch = 1f;
			}

			var src = AcquireSource();
			src.clip = clip;
			src.volume = volumeScale;
			src.pitch = pitch;
			src.loop = true;
			src.Play();

			var id = _nextHandleId++;
			_activeLoops[id] = src;

			this.Log($"PlayLoop '{clip.name}' → handle {id} (pitch={pitch}, vol={volumeScale})");
			return new SfxHandle(id);
		}

		public void StopLoop(SfxHandle handle, float fade = 0f)
		{
			if (!handle.IsValid) return;
			if (!_activeLoops.Remove(handle.Id, out var src))
				return;

			if (!src) return; // source 已被外部销毁

			if (fade <= 0f)
			{
				ReleaseSource(src);
				return;
			}

			src.DOFade(0f, fade)
				.SetTarget(src)
				.OnComplete(() => { if (src) ReleaseSource(src); });
		}

		public void SetLoopPitch(SfxHandle handle, float pitch)
		{
			if (!handle.IsValid || pitch <= 0f) return;
			if (_activeLoops.TryGetValue(handle.Id, out var src) && src)
				src.pitch = pitch;
		}

		public void SetLoopVolume(SfxHandle handle, float volumeScale)
		{
			if (!handle.IsValid) return;
			if (_activeLoops.TryGetValue(handle.Id, out var src) && src)
				src.volume = Mathf.Max(0f, volumeScale);
		}

		public bool IsLoopPlaying(SfxHandle handle)
			=> handle.IsValid && _activeLoops.ContainsKey(handle.Id);

		#endregion

		#region pool

		private AudioSource AcquireSource()
		{
			while (_freeSources.Count > 0)
			{
				var src = _freeSources.Pop();
				if (src) return src;
			}
			return CreateSource();
		}

		private AudioSource CreateSource()
		{
			var go = new GameObject("PooledSfxSource");
			go.transform.SetParent(transform, worldPositionStays: false);

			var src = go.AddComponent<AudioSource>();
			src.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
			src.playOnAwake = false;
			src.spatialBlend = 0f; // 2D 音频
			return src;
		}

		private void ReleaseSource(AudioSource src)
		{
			if (!src) return;

			DOTween.Kill(src);

			src.Stop();
			src.clip = null;
			src.loop = false;
			src.pitch = 1f;
			src.volume = 1f;
			_freeSources.Push(src);
		}

		private IEnumerator ReleaseAfter(AudioSource src, float seconds)
		{
			yield return new WaitForSeconds(seconds);
			if (src) ReleaseSource(src);
		}

		#endregion

		#region Volume

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
				var v01 = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : GetDefaultVolume(ch);
				ApplyVolumeToMixer(ch, v01);
			}
		}

		private void ApplyVolumeToMixer(EVolumeChannel channel, float v01)
		{
			var paramName = GetMixerParamName(channel);
			var db = LinearToDb(v01);
			if (!mixer.SetFloat(paramName, db))
				this.LogError($"Mixer parameter '{paramName}' not exposed. Did you set it in the AudioMixer asset?");
			Debug.Log($"Setting mixer parameter '{paramName}' to '{db}'");
		}

		#endregion

		private static string GetMixerParamName(EVolumeChannel channel) => channel.ToString();

		private float GetDefaultVolume(EVolumeChannel channel)
		{
			return channel switch
			{
				EVolumeChannel.Master => defaultGlobalVolume,
				EVolumeChannel.BGM => defaultBgmVolume,
				EVolumeChannel.SFX => defaultSfxVolume,
				_ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null)
			};
		}

		private static string PrefKey(EVolumeChannel channel) => PrefKeyPrefix + channel;

		private static float LinearToDb(float linear) => linear > 0.0001f ? 20f * Mathf.Log10(linear) : MinDb;

		private static float DbToLinear(float db) => Mathf.Pow(10f, db / 20f);

		#region Debug

		[ShowInInspector, ReadOnly, PropertyOrder(100), FoldoutGroup("Debug")]
		private int ActiveLoopCount => _activeLoops.Count;

		[ShowInInspector, ReadOnly, PropertyOrder(101), FoldoutGroup("Debug")]
		private int FreePoolSize => _freeSources.Count;

		#endregion
	}
}

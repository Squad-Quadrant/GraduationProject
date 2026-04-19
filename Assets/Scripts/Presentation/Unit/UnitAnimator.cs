using System;
using System.Collections;
using Core.Log;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Presentation.Unit
{
	/// <summary>
	/// 对于Spine动画播放的封装
	/// </summary>
	[RequireComponent(typeof(SkeletonAnimation))]
	public class UnitAnimator : MonoBehaviour
	{
		private const int MainTrack = 0;

		private SkeletonAnimation _skeletonAnimation;
		private Skeleton _skeleton;
		private Spine.AnimationState _animState;

		private TrackEntry _currentEntry;
		private Spine.AnimationState.TrackEntryDelegate _pendingCallback;
		private Spine.AnimationState.TrackEntryEventDelegate _pendingCallbackEvent;

		private Coroutine _fidgetCoroutine;

		public bool IsInitialized { get; private set; }

		public bool IsPaused => _skeletonAnimation &&
		                        Mathf.Approximately(_skeletonAnimation.timeScale, 0f);

		public string CurrentAnimationName => _currentEntry?.Animation?.Name;

		public void Initialize(SkeletonDataAsset dataAsset, string bodySkin, string weaponSkin = null)
		{
			_skeletonAnimation = GetComponent<SkeletonAnimation>();

			if (_skeletonAnimation.SkeletonDataAsset != dataAsset)
			{
				_skeletonAnimation.skeletonDataAsset = dataAsset;
				_skeletonAnimation.Initialize(true);
			}
			else
			{
				// already has the correct data asset, just ensure it's initialized
				if (!_skeletonAnimation.valid)
					_skeletonAnimation.Initialize(false);
			}

			_skeleton = _skeletonAnimation.Skeleton;
			_animState = _skeletonAnimation.AnimationState;

			if (!_skeletonAnimation || _skeleton == null || _animState == null)
			{
				this.LogError("Failed to initialize: missing SkeletonAnimation or Skeleton or AnimationState");
				return;
			}

			ApplySkinCombination(bodySkin, weaponSkin);
			IsInitialized = true;

			this.Log($"Initialized - body:'{bodySkin}', weapon:'{weaponSkin ?? "none"}'");
		}

		#region Animation Control

		// immediate play; onComplete is called after the animation finishes each loop
		public void Play(string animName, bool loop, Action onComplete = null)
		{
			if (!ValidateReady()) return;
			StopFidgetCycle();
			PlayInternal(animName, loop, onComplete);
		}

		public void PlayLoopWithFidgets(AnimationResult[] baseClips, AnimationResult[] fidgetClips, float minDelay, float maxDelay)
		{
			if (!ValidateReady()) return;
			StopFidgetCycle();
			_fidgetCoroutine = StartCoroutine(FidgetCycleCoroutine(baseClips, fidgetClips, minDelay, maxDelay));
		}

		public void Queue(string animName, bool loop, float delay = 0f, Action onComplete = null)
		{
			if (!ValidateReady()) return;

			var anim = _skeleton.Data.FindAnimation(animName);
			if (anim == null)
			{
				this.LogError($"Animation '{animName}' not found in skeleton data");
				return;
			}

			var entry = _animState.AddAnimation(MainTrack, anim, loop, delay);

			if (onComplete != null)
			{
				Spine.AnimationState.TrackEntryDelegate callback = null;
				callback = _ =>
				{
					entry.Complete -= callback;
					onComplete.Invoke();
				};
				entry.Complete += callback;
			}

			this.Log($"Queued animation: '{animName}', loop: {loop}, delay: {delay}");
		}

		public void Stop()
		{
			if (!ValidateReady()) return;
			StopFidgetCycle();
			ClearPendingCallback();
			_animState.ClearTrack(MainTrack);
			_currentEntry = null;
		}

		public void Pause()
		{
			if (!_skeletonAnimation) return;
			_skeletonAnimation.timeScale = 0f;
		}

		public void Resume()
		{
			if (!_skeletonAnimation) return;
			_skeletonAnimation.timeScale = 1f;
		}

		public void ListenForSpineEvent(string eventName, Action callback)
		{
			if (!ValidateReady()) return;

			ClearPendingCallbackEvent();

			_pendingCallbackEvent = (entry, e) =>
			{
				if (entry.TrackIndex != MainTrack) return;
				if (e.Data.Name != eventName) return;

				ClearPendingCallbackEvent();
				callback?.Invoke();
			};

			_animState.Event += _pendingCallbackEvent;
		}

		private void PlayInternal(string animName, bool loop, Action onComplete = null)
		{
			ClearPendingCallback();
			ClearPendingCallbackEvent();

			var anim = _skeleton.Data.FindAnimation(animName);
			if (anim == null)
			{
				this.LogError($"Animation '{animName}' not found in skeleton data");
				return;
			}

			_currentEntry = _animState.SetAnimation(MainTrack, anim, loop);
			if (loop && anim.Duration > 0f) // 错开循环动画
				_currentEntry.TrackTime = UnityEngine.Random.Range(0f, anim.Duration);

			if (onComplete == null) return;

			_pendingCallback = _ =>
			{
				_pendingCallback = null;
				onComplete.Invoke();
			};
			_currentEntry.Complete += _pendingCallback;
		}

		private IEnumerator FidgetCycleCoroutine(AnimationResult[] baseClips, AnimationResult[] fidgetClips,
			float minDelay, float maxDelay)
		{
			while (true)
			{
				var baseClip = baseClips[UnityEngine.Random.Range(0, baseClips.Length)];
				PlayInternal(baseClip.ClipName, true);

				yield return new WaitForSeconds(UnityEngine.Random.Range(minDelay, maxDelay));

				var fidget = fidgetClips[UnityEngine.Random.Range(0, fidgetClips.Length)];

				bool done = false;
				PlayInternal(fidget.ClipName, false, () => done = true);
				yield return new WaitUntil(() => done);
			}
		}

		private void StopFidgetCycle()
		{
			if (_fidgetCoroutine == null) return;
			StopCoroutine(_fidgetCoroutine);
			_fidgetCoroutine = null;
		}

		private void ClearPendingCallback()
		{
			if (_pendingCallback != null && _currentEntry != null)
				_currentEntry.Complete -= _pendingCallback;
			_pendingCallback = null;
		}

		private void ClearPendingCallbackEvent()
		{
			if (_pendingCallbackEvent != null && _animState != null)
				_animState.Event -= _pendingCallbackEvent;
			_pendingCallbackEvent = null;
		}

		private bool ValidateReady()
		{
			if (IsInitialized) return true;
			this.LogWarning("Not initialized. Call Initialize() first.");
			return false;
		}

		#endregion

		#region Skin Management

		private string _currentBodySkin;
		private string _currentWeaponSkin;

		public void SetBodySkin(string skinName)
		{
			if (_currentBodySkin == skinName) return;
			ApplySkinCombination(skinName, _currentWeaponSkin);
		}

		public void SetWeaponSkin(string skinName)
		{
			if (_currentWeaponSkin == skinName) return;
			ApplySkinCombination(_currentBodySkin, skinName);
		}

		private void ApplySkinCombination(string bodySkin, string weaponSkin = null)
		{
			var skeletonData = _skeleton.Data;

			var body = skeletonData.FindSkin(bodySkin);
			if (body == null)
			{
				this.LogError($"Body skin '{bodySkin}' not found in skeleton data");
				return;
			}

			var combined = new Skin($"combined: {bodySkin}_{weaponSkin}");
			combined.AddSkin(body);

			if (!string.IsNullOrEmpty(weaponSkin))
			{
				var weapon = skeletonData.FindSkin(weaponSkin);
				if (weapon == null)
					this.LogError($"Weapon skin '{weaponSkin}' not found in skeleton data");
				else
					combined.AddSkin(weapon);
			}

			_skeleton.SetSkin(combined);
			_skeleton.SetSlotsToSetupPose();

			_currentBodySkin = bodySkin;
			_currentWeaponSkin = weaponSkin;
			this.Log($"Applied skin combination - body:'{bodySkin}', weapon:'{weaponSkin ?? "none"}'");
		}

		#endregion

		#region Facing

		public void SetFaceRight(bool faceRight)
		{
			if (_skeleton == null) return;
			_skeleton.ScaleX = faceRight ? 1f : -1f;
		}

		public bool IsFacingRight => _skeleton is { ScaleX: > 0f };

		#endregion

		#region Debug

		[TitleGroup("Debug — Playback")]
		[ShowInInspector, ReadOnly, LabelText("Current Anim")]
		private string DbgAnim => _currentEntry?.Animation?.Name ?? "none";

		[TitleGroup("Debug — Playback")]
		[ShowInInspector, ReadOnly, LabelText("Looping")]
		private bool DbgLoop => _currentEntry?.Loop ?? false;

		[TitleGroup("Debug — Playback")]
		[ShowInInspector, ReadOnly, LabelText("Progress")]
		[ProgressBar(0, 1, ColorGetter = "DbgBarColor")]
		private float DbgProgress
		{
			get
			{
				if (_currentEntry?.Animation == null) return 0f;
				var dur = _currentEntry.Animation.Duration;
				return dur > 0f ? Mathf.Repeat(_currentEntry.TrackTime, dur) / dur : 0f;
			}
		}

		private Color DbgBarColor => new(0.3f, 0.8f, 1f);

		[TitleGroup("Debug — Playback")]
		[ShowInInspector, ReadOnly, LabelText("Paused")]
		[GUIColor("@DbgPaused ? new Color(1f, 0.8f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
		private bool DbgPaused => IsPaused;

		[TitleGroup("Debug — Skin")]
		[ShowInInspector, ReadOnly, LabelText("Body")]
		private string DbgBody => _currentBodySkin ?? "none";

		[TitleGroup("Debug — Skin")]
		[ShowInInspector, ReadOnly, LabelText("Weapon")]
		private string DbgWeapon => _currentWeaponSkin ?? "none";

		[TitleGroup("Debug — Facing")]
		[ShowInInspector, ReadOnly, LabelText("Scale X")]
		private float DbgScaleX => _skeleton?.ScaleX ?? 1f;

		#endregion
	}
}

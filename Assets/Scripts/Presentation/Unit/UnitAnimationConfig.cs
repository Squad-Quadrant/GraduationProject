using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Spine.Unity;
using Systems.Unit.Equipment.Config;
using UnityEngine;

namespace Presentation.Unit
{
	public enum EUnitStance
	{
		Stand,
		Bend,
		Aim
	}

	public enum EGripType
	{
        [InspectorName("默认(双手)")]
		Default, // 双手
        [InspectorName("手枪(单手)")]
		HandGun // 单手
	}

	[Serializable]
	public struct AnimationEntry
	{
#if UNITY_EDITOR
		private static readonly string[] ActionKeys =
		{
			"idle", "shoot", "reload", "beHit", "throw",
			"move_start", "move_loop", "move_end",
			"hitdown", "dead"
		};
		[ValueDropdown(nameof(ActionKeys))]
#endif
		public string action;

		public EUnitStance stance;

		public EGripType grip;

#if UNITY_EDITOR
		[ValueDropdown(nameof(AllWeaponKeysEditor), IsUniqueList = true)]
#endif
		public List<string> weaponKeys;

		[SpineAnimation(dataField: "skeletonDataAsset")]
		public string clipName;

		public bool loop;

		public bool Matches(string a, EUnitStance s, EGripType g, string w) =>
			KeyEquals(action, a) && stance == s && grip == g && WeaponKeyMatches(w);

		private static bool KeyEquals(string a, string b) =>
			string.IsNullOrEmpty(a) ? string.IsNullOrEmpty(b) : a == b;

		private bool WeaponKeyMatches(string w)
		{
			bool hasKey = weaponKeys != null && weaponKeys.Any(k => !string.IsNullOrEmpty(k));
			return hasKey
				? !string.IsNullOrEmpty(w) && weaponKeys.Contains(w)
				: string.IsNullOrEmpty(w);
		}

#if UNITY_EDITOR
		private static IEnumerable<string> AllWeaponKeysEditor()
		{
			var keys = new SortedSet<string>();
			var guids = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(WeaponConfig));
			foreach (var guid in guids)
			{
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponConfig>(path);
				if (asset && !string.IsNullOrEmpty(asset.animKey))
					keys.Add(asset.animKey);
			}
			return keys;
		}
#endif
	}

	[Serializable]
	public struct TransitionEntry
	{
		public EUnitStance from;
		public EUnitStance to;
		public EGripType grip;

		[SpineAnimation(dataField: "skeletonDataAsset")]
		public string clipName;

		public bool Matches(EUnitStance f, EUnitStance t, EGripType g) =>
			from == f && to == t && grip == g;
	}

	public readonly struct AnimationResult
	{
		public readonly string ClipName;
		public readonly bool Loop;

		public AnimationResult(string clipName, bool loop)
		{
			ClipName = clipName;
			Loop = loop;
		}

		public bool IsValid => !string.IsNullOrEmpty(ClipName);
		public static readonly AnimationResult Empty = new(null, false);
	}

	public readonly struct IdleSet // 为了小动作的播放
	{
		public readonly AnimationResult[] BaseClips;
		public readonly AnimationResult[] FidgetClips;

		public IdleSet(AnimationResult[] baseClips, AnimationResult[] fidgetClips)
		{
			BaseClips = baseClips;
			FidgetClips = fidgetClips;
		}

		public bool IsValid => BaseClips is { Length: > 0 };
		public bool HasFidgets => FidgetClips is { Length: > 0 };
		public static readonly IdleSet Empty = new(null, null);
	}

	[CreateAssetMenu(fileName = "NewUnitAnimConfig", menuName = "Game/Unit/Unit Animation Config")]
	public class UnitAnimationConfig : ScriptableObject
	{
		[TitleGroup("Defaults")]
		[SerializeField] private EUnitStance defaultStance = EUnitStance.Stand;

		[TitleGroup("Defaults")]
		[SerializeField] private EGripType defaultGrip = EGripType.Default;

		[TitleGroup("Defaults"), LabelText("Stance无关动作")]
		[SerializeField] private List<string> stanceAgnosticActions = new() { "shoot", "reload" };
		private HashSet<string> _agnostic;
		private bool IsStanceAgnostic(string a) =>
			(_agnostic ??= new HashSet<string>(stanceAgnosticActions)).Contains(a);

		[TitleGroup("Fidget")]
		[SerializeField, MinMaxSlider(1f, 30f, ShowFields = true)]
		private Vector2 fidgetInterval = new(5f, 12f);

		[TitleGroup("Animations")]
		[SerializeField, TableList(ShowIndexLabels = true)] private List<AnimationEntry> animations = new();

		[TitleGroup("Transitions")]
		[SerializeField, TableList(ShowIndexLabels = true)] private List<TransitionEntry> transitions = new();

		[Title("Audio")] // 先放在这里，比较方便
		[LabelText("脚步")] public AudioClip footstepClip;
		[LabelText("脚步速度"), Range(0.5f, 3f)] public float footstepPitch = 1f;

		#region Inspector Helper

		[TitleGroup("Animations"), HorizontalGroup("Animations/Move")]
		[SerializeField, LabelText("Row"), LabelWidth(30), PropertyRange(0, nameof(AnimMaxIndex))]
		private int animIndex;

		private int AnimMaxIndex => Mathf.Max(0, animations.Count - 1);

		[TitleGroup("Animations"), HorizontalGroup("Animations/Move", width: 30)]
		[Button("↑")]
		private void AnimMoveUp()
		{
			if (animIndex <= 0 || animIndex >= animations.Count) return;
			(animations[animIndex], animations[animIndex - 1]) = (animations[animIndex - 1], animations[animIndex]);
			animIndex--;
		}

		[TitleGroup("Animations"), HorizontalGroup("Animations/Move", width: 30)]
		[Button("↓")]
		private void AnimMoveDown()
		{
			if (animIndex >= animations.Count - 1) return;
			(animations[animIndex], animations[animIndex + 1]) = (animations[animIndex + 1], animations[animIndex]);
			animIndex++;
		}


		[TitleGroup("Transitions"), HorizontalGroup("Transitions/Move")]
		[SerializeField, LabelText("Row"), LabelWidth(30), PropertyRange(0, nameof(TranMaxIndex))]
		private int transIndex;

		private int TranMaxIndex => Mathf.Max(0, transitions.Count - 1);

		[TitleGroup("Transitions"), HorizontalGroup("Transitions/Move", width: 30)]
		[Button("↑")]
		private void TransMoveUp()
		{
			if (transIndex <= 0 || transIndex >= transitions.Count) return;
			(transitions[transIndex], transitions[transIndex - 1]) = (transitions[transIndex - 1], transitions[transIndex]);
			transIndex--;
		}

		[TitleGroup("Transitions"), HorizontalGroup("Transitions/Move", width: 30)]
		[Button("↓")]
		private void TransMoveDown()
		{
			if (transIndex >= transitions.Count - 1) return;
			(transitions[transIndex], transitions[transIndex + 1]) = (transitions[transIndex + 1], transitions[transIndex]);
			transIndex++;
		}


		[TitleGroup("Validation")]
		[SerializeField] private SkeletonDataAsset skeletonDataAsset;

		[TitleGroup("Validation")]
		[Button]
		private void Validate()
		{
			if (!skeletonDataAsset)
			{
				Debug.LogWarning("Skeleton data asset not set");
				return;
			}

			var skeletonData = skeletonDataAsset.GetSkeletonData(false);
			if (skeletonData == null)
			{
				Debug.LogWarning("Skeleton data asset not set");
				return;
			}

			var skeletonAnimations = skeletonData.Animations.Select(a => a.Name).ToHashSet();

			Debug.Log("Checking Animations:");
			for (int i = 0; i < animations.Count; i++)
			{
				var animation = animations[i];
				if (string.IsNullOrEmpty(animation.clipName))
				{
					Debug.LogWarning($"{i}: Animation clip name not set");
					continue;
				}
				if (string.IsNullOrEmpty(animation.action))
					Debug.LogWarning($"{i}: Action not set");

				if (!skeletonAnimations.Contains(animation.clipName))
					Debug.LogError($"{i}: clip name {animation.clipName} not exist");
				else
					skeletonAnimations.Remove(animation.clipName);
			}

			Debug.Log("==================================");
			Debug.Log("Checking Transitions:");
			for (int i = 0; i < transitions.Count; i++)
			{
				var transition = transitions[i];
				if (string.IsNullOrEmpty(transition.clipName))
				{
					Debug.LogWarning($"{i}: Transition clip name not set");
					continue;
				}

				if (!skeletonAnimations.Contains(transition.clipName))
					Debug.LogError($"{i}: clip name '{transition.clipName}' not exist");
				else
					skeletonAnimations.Remove(transition.clipName);
			}

			Debug.Log("==================================");
			if (skeletonAnimations.Count == 0) return;
			var msg = skeletonAnimations
				.Aggregate("Clip Name that have not use yet: \n", (current, animation) => current + $"{animation}\n");
			Debug.LogWarning(msg);
		}

		#endregion

		public EUnitStance DefaultStance => defaultStance;
		public EGripType DefaultGrip => defaultGrip;
		public float FidgetMinDelay => fidgetInterval.x;
		public float FidgetMaxDelay => fidgetInterval.y;

		private static readonly System.Random SharedRandom = new();

		// 如果有多个相同状态相同名字的，会随机返回一个
		public AnimationResult GetAnimation(string action, EUnitStance stance, EGripType grip, string weaponKey)
		{
			if (IsStanceAgnostic(action)) stance = defaultStance;

			var result = MatchAnimation(action, stance, grip, weaponKey);
			if (result.IsValid) return result;

			result = MatchAnimation(action, stance, grip, null);
			if (result.IsValid) return result;

			result = MatchAnimation(action, stance, defaultGrip, null);
			if (result.IsValid) return result;

			result = MatchAnimation(action, defaultStance, defaultGrip, null);
			if (result.IsValid) return result;

			return AnimationResult.Empty;
		}

		public string GetTransition(EUnitStance from, EUnitStance to, EGripType grip)
		{
			if (from == to) return null;

			var result = MatchTransition(from, to, grip);
			if (!string.IsNullOrEmpty(result)) return result;

			if (grip != defaultGrip)
				return MatchTransition(from, to, defaultGrip);

			return null;
		}

		public IdleSet GetIdleSet(EUnitStance stance, EGripType grip, string weaponKey)
		{
			if (!string.IsNullOrEmpty(weaponKey))
			{
				var s = MatchIdleSet(stance, grip, weaponKey);
				if (s.IsValid) return s;
			}

			var set = MatchIdleSet(stance, grip, null);
			if (set.IsValid) return set;

			if (grip != defaultGrip)
			{
				set = MatchIdleSet(stance, defaultGrip, null);
				if (set.IsValid) return set;
			}

			if (stance != defaultStance || grip != defaultGrip)
			{
				set = MatchIdleSet(defaultStance, defaultGrip, null);
				if (set.IsValid) return set;
			}

			return IdleSet.Empty;
		}

		private AnimationResult MatchAnimation(string action, EUnitStance stance, EGripType grip, string weaponKey)
		{
			var matches = animations.Where(entry => entry.Matches(action, stance, grip, weaponKey)).ToList();
			if (matches.Count == 0) return AnimationResult.Empty;

			var picked = matches[SharedRandom.Next(matches.Count)];
			return new AnimationResult(picked.clipName, picked.loop);
		}

		private string MatchTransition(EUnitStance from, EUnitStance to, EGripType grip)
		{
			return transitions
				.Where(entry => entry.Matches(from, to, grip))
				.Select(entry => entry.clipName)
				.FirstOrDefault();
		}

		private IdleSet MatchIdleSet(EUnitStance stance, EGripType grip, string weaponKey)
		{
			var matches = animations.Where(e => e.Matches("idle", stance, grip, weaponKey)).ToList();
			if (matches.Count == 0) return IdleSet.Empty;

			var bases = matches.Where(e => e.loop)
				.Select(e => new AnimationResult(e.clipName, true)).ToArray();
			var fidgets = matches.Where(e => !e.loop)
				.Select(e => new AnimationResult(e.clipName, false)).ToArray();

			// Need at least one base clip to be valid
			return bases.Length > 0 ? new IdleSet(bases, fidgets) : IdleSet.Empty;
		}

		#region Editor Display

		[ShowInInspector, DisplayAsString, HideLabel]
		[PropertyOrder(-1)]
		private string EditorTitle => $"Animation Config: {name}";

		#endregion
	}
}

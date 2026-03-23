using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Presentation.Unit
{
	public enum EUnitStance
	{
		Stand,
		Bend,
		Aim,
		Hit
	}

	public enum EGripType
	{
		Default,
		HandGun
	}

	[Serializable]
	public struct AnimationEntry
	{
		public string action;
		public EUnitStance stance;
		public EGripType grip;
		public string clipName;
		public bool loop;

		public bool Matches(string a, EUnitStance s, EGripType g) =>
			action == a && stance == s && grip == g;
	}

	[Serializable]
	public struct TransitionEntry
	{
		public EUnitStance from;
		public EUnitStance to;
		public EGripType grip;
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

	[CreateAssetMenu(fileName = "NewUnitAnimConfig", menuName = "Game/Unit Animation Config")]
	public class UnitAnimationConfig : ScriptableObject
	{
		[TitleGroup("Defaults")]
		[SerializeField] private EUnitStance defaultStance = EUnitStance.Stand;

		[TitleGroup("Defaults")]
		[SerializeField] private EGripType defaultGrip = EGripType.Default;

		[TitleGroup("Animations")]
		[SerializeField, TableList(ShowIndexLabels = true)] private List<AnimationEntry> animations = new();

		[TitleGroup("Transitions")]
		[SerializeField, TableList(ShowIndexLabels = true)] private List<TransitionEntry> transitions = new();

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

		public AnimationResult GetAnimation(string action, EUnitStance stance, EGripType grip)
		{
			var result = MatchAnimation(action, stance, grip);
			if (result.IsValid) return result;

			if (grip != defaultGrip)
			{
				result = MatchAnimation(action, stance, defaultGrip);
				if (result.IsValid) return result;
			}

			if (stance != defaultStance || grip != defaultGrip)
			{
				result = MatchAnimation(action, defaultStance, defaultGrip);
				if (result.IsValid) return result;
			}

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

		private AnimationResult MatchAnimation(string action, EUnitStance stance, EGripType grip)
		{
			foreach (var entry in animations.Where(entry => entry.Matches(action, stance, grip)))
				return new AnimationResult(entry.clipName, entry.loop);
			return AnimationResult.Empty;
		}

		private string MatchTransition(EUnitStance from, EUnitStance to, EGripType grip)
		{
			return transitions
				.Where(entry => entry.Matches(from, to, grip))
				.Select(entry => entry.clipName)
				.FirstOrDefault();
		}

		#region Editor Display

		[ShowInInspector, DisplayAsString, HideLabel]
		[PropertyOrder(-1)]
		private string EditorTitle => $"Animation Config: {name}";

		#endregion
	}
}

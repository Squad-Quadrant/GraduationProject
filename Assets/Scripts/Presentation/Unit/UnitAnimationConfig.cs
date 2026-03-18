using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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
		[SerializeField, TableList] private List<AnimationEntry> animations = new();

		[TitleGroup("Transitions")]
		[SerializeField, TableList] private List<TransitionEntry> transitions = new();

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

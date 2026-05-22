using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Dialogue.Config
{
	[Serializable]
	public class PortraitEntry
	{
		[LabelText("角色")]
		[Required]
		public CharacterConfig character;

		[LabelText("位置")]
		public EPortraitPosition position;

		[ValueDropdown(nameof(GetPoseOptions), AppendNextDrawer = true)]
		[Tooltip("留空 = 使用角色 defaultPoseName")]
		public string poseOverride = string.Empty;

		public string Pose => string.IsNullOrEmpty(poseOverride) ? character.defaultPoseName : poseOverride;

		[ValueDropdown(nameof(GetSkinOptions), AppendNextDrawer = true)]
		[Tooltip("留空 = 使用角色 defaultSkinName。仅 Spine 立绘有效。")]
		public string skinOverride = string.Empty;

		public string Skin => string.IsNullOrEmpty(skinOverride) ? character.defaultSkinName : skinOverride;

		[LabelText("一次性动画")]
		[ValueDropdown(nameof(GetPoseOptions), AppendNextDrawer = true)]
		[Tooltip("入此节点稳态前先播一次的动画，仅 Spine 立绘有效")]
		public string oneShotPose;

		private IEnumerable<string> GetPoseOptions()
		{
			if (!character) yield break;
			foreach (var p in character.GetAvailablePoses()) yield return p;
		}

		private IEnumerable<string> GetSkinOptions()
		{
			if (!character) yield break;
			foreach (var s in character.GetAvailableSkins()) yield return s;
		}
	}

	[Serializable]
	public class DialogueNode
	{
		[LabelText("说话人")]
		[InfoBox("留空 = 旁白", InfoMessageType.None, "@speaker == null")]
		public CharacterConfig speaker;

		[LabelText("台词")]
		[TextArea(4, 10)]
		public string text;

		[LabelText("舞台立绘")]
		[InfoBox("当前节点舞台上的全部立绘。", InfoMessageType.None)]
		[TableList(ShowIndexLabels = false, AlwaysExpanded = true, DrawScrollView = false)]
		public List<PortraitEntry> portraits = new();
	}

	[CreateAssetMenu(fileName = "NewDialogue", menuName = "Game/Dialogue/Dialogue Config")]
	public class DialogueConfig : ScriptableObject
	{
		[LabelText("对话 ID")]
		public string dialogueId = "dialogue_001";

		[LabelText("对话节点")]
		[ListDrawerSettings(
			ShowFoldout = true,
			ShowPaging = true,
			NumberOfItemsPerPage = 10,
			ShowIndexLabels = true,
			CustomAddFunction = nameof(CreateNodeInheritingPrevious))]
		public List<DialogueNode> nodes = new();

		private DialogueNode CreateNodeInheritingPrevious()
		{
			var node = new DialogueNode();
			if (nodes.Count == 0) return node;

			var prev = nodes[^1];
			if (prev.portraits == null || prev.portraits.Count == 0) return node;

			node.portraits = new List<PortraitEntry>(prev.portraits.Count);
			foreach (var p in prev.portraits)
			{
				node.portraits.Add(new PortraitEntry
				{
					character    = p.character,
					position     = p.position,
					poseOverride = p.poseOverride,
					skinOverride = p.skinOverride,
				});
			}
			return node;
		}
	}
}

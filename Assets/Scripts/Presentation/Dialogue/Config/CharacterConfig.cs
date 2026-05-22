using System.Collections.Generic;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Presentation.Dialogue.Config
{
	[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Dialogue/Character Config")]
	public class CharacterConfig : ScriptableObject
	{
		[Title("基础信息")]
		[LabelText("角色 ID")]
		public string characterId = "char_001";

		[LabelText("显示名")]
		public string displayName = "角色001";

		[Title("立绘")]
		[LabelText("立绘模式")]
		[EnumToggleButtons]
		public EPortraitMode mode = EPortraitMode.Spine;

		[LabelText("立绘预制体")]
		[Required, AssetsOnly]
		public GameObject portraitPrefab;

		[ShowIf("mode", EPortraitMode.Spine)]
		[Required]
		public SkeletonDataAsset skeletonDataAsset;

		[LabelText("默认 Pose")]
		[ValueDropdown(nameof(GetAvailablePoses), AppendNextDrawer = true)]
		public string defaultPoseName = "idle";

		[ShowIf("mode", EPortraitMode.Spine)]
		[LabelText("默认 Spine Skin")]
		[ValueDropdown(nameof(GetAvailableSkins), AppendNextDrawer = true)]
		public string defaultSkinName = "default";

		[Title("舞台动画风格", bold: true)]
		[LabelText("入场风格")]
		public EEntranceStyle entranceStyle = EEntranceStyle.Fade;

		[LabelText("退场风格")]
		public EExitStyle exitStyle = EExitStyle.Fade;

		public IEnumerable<string> GetAvailablePoses()
		{
			if (mode != EPortraitMode.Spine || !skeletonDataAsset) yield break;
			var data = skeletonDataAsset.GetSkeletonData(true);
			if (data == null) yield break;
			foreach (var anim in data.Animations) yield return anim.Name;
		}

		public IEnumerable<string> GetAvailableSkins()
		{
			if (mode != EPortraitMode.Spine || !skeletonDataAsset) yield break;
			var data = skeletonDataAsset.GetSkeletonData(true);
			if (data == null) yield break;
			foreach (var skin in data.Skins) yield return skin.Name;
		}
	}
}

using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Presentation.UI.Component.UnitPortrait
{
	public class SpineUnitPortraitView : UnitPortraitView
	{
		[SerializeField, Required, ChildGameObjectsOnly]
		private SkeletonGraphic skeletonGraphic;

		[Tooltip("Init 时启动的循环动画名。留空则保持 setup pose 静止。")]
		[SerializeField] private string animationName = "portrait_idle";

		[Tooltip("使用的 Spine skin 名。留空则用骨架的默认 skin。")]
		[SerializeField] private string skinName;

		public override void Init()
		{
			if (!skeletonGraphic) return;

			if (!string.IsNullOrEmpty(skinName))
			{
				skeletonGraphic.Skeleton.SetSkin(skinName);
				skeletonGraphic.Skeleton.SetSlotsToSetupPose();
			}

			if (!string.IsNullOrEmpty(animationName))
				skeletonGraphic.AnimationState.SetAnimation(0, animationName, true);
		}
	}
}

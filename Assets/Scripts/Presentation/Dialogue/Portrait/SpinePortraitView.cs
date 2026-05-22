using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Presentation.Dialogue.Portrait
{
	public class SpinePortraitView : PortraitViewBase
	{
		[TitleGroup("References")]
		[SerializeField, Required, ChildGameObjectsOnly]
		private SkeletonGraphic skeletonGraphic;

		private const int MainTrack = 0;

		protected override void ApplyIdle(string poseName, string skinName)
		{
			if (!skeletonGraphic) return;

			ApplySkin(skinName);

			if (!string.IsNullOrEmpty(poseName))
				skeletonGraphic.AnimationState.SetAnimation(MainTrack, poseName, true);
		}

		protected override void PlayOneShotThenIdle(string oneShotAnim, string followingPose, string followingSkin)
		{
			if (!skeletonGraphic) return;

			ApplySkin(followingSkin);

			if (!string.IsNullOrEmpty(oneShotAnim))
				skeletonGraphic.AnimationState.SetAnimation(MainTrack, oneShotAnim, false);

			if (!string.IsNullOrEmpty(followingPose))
				skeletonGraphic.AnimationState.AddAnimation(MainTrack, followingPose, true, 0f);
		}

		private void ApplySkin(string skinName)
		{
			if (string.IsNullOrEmpty(skinName)) return;
			skeletonGraphic.Skeleton.SetSkin(skinName);
			skeletonGraphic.Skeleton.SetSlotsToSetupPose();
		}
	}
}

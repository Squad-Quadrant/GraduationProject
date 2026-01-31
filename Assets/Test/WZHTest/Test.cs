using Core.Log;
using Presentation.Bootstrap;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Test.WZHTest
{
	public class Test : MonoBehaviour
	{
		[SerializeField] private SkeletonAnimation skeletonAnimation;
		[SerializeField] private string characterSkinName = "pcb front";
		[SerializeField] private string weaponSkinName = "weapon_a1";

		[Button]
		private void Foo()
		{
			var combinedSkin = new Skin("combined-skin");
			var characterSkin = skeletonAnimation.Skeleton.Data.FindSkin(characterSkinName);
			var weaponSkin = skeletonAnimation.Skeleton.Data.FindSkin(weaponSkinName);
			if (characterSkin == null || weaponSkin == null)
			{
				Debug.LogError("One or both skins not found.");
				return;
			}
			combinedSkin.AddSkin(characterSkin);
			combinedSkin.AddSkin(weaponSkin);
			skeletonAnimation.Skeleton.SetSkin(combinedSkin);
			skeletonAnimation.Skeleton.SetSlotsToSetupPose();
		}
	}
}

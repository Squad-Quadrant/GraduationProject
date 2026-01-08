using Core.Log;
using Presentation.Bootstrap;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Test.WZHTest
{
	public class Test : MonoBehaviour
	{
		[SerializeField] private SkeletonAnimation skeletonAnimation;

		[Button]
		private void Foo()
		{
			skeletonAnimation.Skeleton.SetAttachment("BG", null);
		}
	}
}

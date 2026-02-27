using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Test.WZHTest
{
	public class Test : MonoBehaviour
	{
		[SerializeField] private SkeletonAnimation skeletonAnimation;
		[SerializeField] private bool facingRight = true;

		[Button]
		private void Foo()
		{
			skeletonAnimation.Skeleton.ScaleX = facingRight ? 1 : -1;
		}
	}
}

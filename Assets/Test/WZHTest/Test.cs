using Core.Log;
using Presentation.Bootstrap;
using Presentation.UI.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Test.WZHTest
{
	public class Test : MonoBehaviour
	{
		[Button]
		private void Foo()
		{
			var manager = RootContainer.Instance.Resolve<UIManager>();
			if (manager)
			{
				this.LogDebug("Found UIManager via RootContainer.");

				this.LogDebug($"{manager.Navigator.Count}");
			}
			else
			{
				this.LogDebug("UIManager not found in RootContainer.");
			}
		}
	}
}

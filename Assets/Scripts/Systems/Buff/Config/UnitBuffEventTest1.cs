using UnityEngine;

namespace Systems.Buff.Config
{
	[CreateAssetMenu(fileName = "UnitBuffEventTest1", menuName = "Game/Buff/UnitBuffEvent/UnitBuffEventTest1")]
	public class UnitBuffEventTest1 : UnitBuffEvent
	{
		public override void Trigger(BuffInfo buffInfo)
		{
			base.Trigger(buffInfo);
			Debug.Log("BuffEventTest1");
		}
	}
}

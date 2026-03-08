using UnityEngine;

namespace Systems.Buff.Config
{
	[CreateAssetMenu(fileName = "BuffEventTest1", menuName = "Game/BuffEvent/BuffEventTest1")]
	public class BuffEventTest1 : BuffEvent
	{
		public override void Trigger(BuffInfo buffInfo)
		{
			Debug.Log("BuffEventTest1");
		}
	}
}

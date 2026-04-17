using Core.Log;
using UnityEngine;

namespace Systems.Buff.Config
{
	[CreateAssetMenu(fileName = "UnitBuffEventTest1", menuName = "Game/Buff/BuffEvent/UnitBuffEventTest1")]
	public class PercentHp : UnitBuffEvent
	{
		public float percentage;
		protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
		{
			
		}
	}
}

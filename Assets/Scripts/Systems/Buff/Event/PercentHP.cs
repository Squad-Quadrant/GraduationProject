using Core.Log;
using UnityEngine;

namespace Systems.Buff.Config
{
	[CreateAssetMenu(fileName = "PercentHp", menuName = "Game/Buff/BuffEvent/PercentHp")]
	public class PercentHp : UnitBuffEvent
	{
		public float percentage;
		protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
		{
			
		}
	}
}

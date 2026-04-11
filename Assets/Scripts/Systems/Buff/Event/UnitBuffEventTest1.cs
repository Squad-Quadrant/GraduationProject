using Core.Log;
using UnityEngine;

namespace Systems.Buff.Config
{
	[CreateAssetMenu(fileName = "UnitBuffEventTest1", menuName = "Game/Buff/UnitBuffEvent/UnitBuffEventTest1")]
	public class UnitBuffEventTest1 : UnitBuffEvent
	{
		protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
		{
			this.Log($"UnitBuffEventTest1 Triggered! BuffId: {buffInfo.Name}, UnitId: {unit.id}", true);
		}
	}
}

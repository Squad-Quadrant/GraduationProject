using UnityEngine;

namespace Systems.Buff.Influence
{
    [CreateAssetMenu(fileName = "BoolChanger", menuName = "Game/Buff/BuffInfluence/BoolChanger")]
    public class BoolChanger : UnitBuffInfluence<bool>
    {
        public bool can;
        protected override void Execute(BuffInfo buffInfo, BuffProperty<bool> property, Unit.Unit unit)
        {
            property.buffValue = can;
        }
    }
}
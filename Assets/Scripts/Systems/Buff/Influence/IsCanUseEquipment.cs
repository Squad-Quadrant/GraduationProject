using UnityEngine;

namespace Systems.Buff.Influence
{
    [CreateAssetMenu(fileName = "IsCanUseEquipment", menuName = "Game/Buff/BuffInfluence/IsCanUseEquipment")]
    public class IsCanUseEquipment : UnitBuffInfluence<bool>
    {
        public bool can;
        protected override void Execute(BuffInfo buffInfo, BuffProperty<bool> property, Unit.Unit unit)
        {
            property.buffValue = can;
        }
    }
}
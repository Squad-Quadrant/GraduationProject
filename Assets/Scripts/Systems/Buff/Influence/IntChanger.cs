using UnityEngine;

namespace Systems.Buff.Influence
{
    [CreateAssetMenu(fileName = "IntChanger", menuName = "Game/Buff/BuffInfluence/IntChanger")]
    public class IntChanger : UnitBuffInfluence<int>
    {
        public int changer = 0;
        protected override void Execute(BuffInfo buffInfo, BuffProperty<int> property, Unit.Unit unit)
        {
            property.buffValue += changer; 
        }
    }
}
using UnityEngine;

namespace Systems.Buff.Influence
{
    [CreateAssetMenu(fileName = "IntMultiplier", menuName = "Game/Buff/BuffInfluence/IntMultiplier")]
    public class IntMultiplier : UnitBuffInfluence<int>
    {
        public float multiplier = 1;
        protected override void Execute(BuffInfo buffInfo, BuffProperty<int> property, Unit.Unit unit)
        {
            property.buffValue = Mathf.FloorToInt(property.buffValue * multiplier);
        }
    }
}
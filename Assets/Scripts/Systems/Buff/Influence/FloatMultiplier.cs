using UnityEngine;

namespace Systems.Buff.Influence
{
    [CreateAssetMenu(fileName = "FloatMultiplier", menuName = "Game/Buff/BuffInfluence/FloatMultiplier")]
    public class FloatMultiplier : UnitBuffInfluence<float>
    {
        public float multiplier = 1;
        protected override void Execute(BuffInfo buffInfo, BuffProperty<float> property, Unit.Unit unit)
        {
            property.buffValue *= multiplier;
        }
    }
}
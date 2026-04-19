using Data.Runtime.Events.Vision;
using UnityEngine;

namespace Systems.Buff.Influence
{
    [CreateAssetMenu(fileName = "VisionMultiplier", menuName = "Game/Buff/BuffInfluence/VisionMultiplier")]
    public class VisionMultiplier : UnitBuffInfluence<int>
    {
        public float multiplier = 1;
        protected override void Execute(BuffInfo buffInfo, BuffProperty<int> property, Unit.Unit unit)
        {
            property.buffValue = Mathf.FloorToInt(property.buffValue * multiplier);
        }
    }
}
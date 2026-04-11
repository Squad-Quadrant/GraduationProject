using Core.Log;

namespace Systems.Buff.Influence
{
    public class UnitBuffInfluenceTest1 : UnitBuffInfluence<int>
    {
        protected override void Execute(BuffInfo buffInfo, BuffProperty<int> property, Unit.Unit unit)
        {
            property.buffValue++;
            this.Log($"UnitBuffInfluenceTest1 Executed! BuffId: {buffInfo.Name}, UnitId: {unit.id}, PropertyType: {property.Type}, BuffValue: {property.buffValue}", true);
        }
    }
}
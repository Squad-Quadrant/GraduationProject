namespace Systems.Unit.Equipment
{
    public static class EquipmentContainerExtensions
    {
        public static bool IsNullOrEmpty(this EquipmentContainer container)
        {
            return container == null || container.Config == null;
        }
    }
}

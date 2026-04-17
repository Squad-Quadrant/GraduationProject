using System.Collections.Generic;

namespace Systems.Unit.Equipment
{
    public interface IEquipable
    {
        public EquipmentContainer MainWeapon { get; set; }
        public EquipmentContainer SecondaryWeapon { get; set; }
        public EquipmentContainer TacticalItem0 { get; set; }
        public EquipmentContainer TacticalItem1 { get; set; }
        public EquipmentContainer TacticalItem2 { get; set; }
        
        public List<EquipmentContainer> TacticalItemInfos { get; }

        // public void InitEquipment(List<EquipmentConfig> equipments);
        // {
        //     TacticalItemInfo0,
        //     TacticalItemInfo1,
        //     TacticalItemInfo2
        // };
    }
}

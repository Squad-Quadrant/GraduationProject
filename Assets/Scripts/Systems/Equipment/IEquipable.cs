using System.Collections.Generic;
using Systems.Equipment.Config;

namespace Systems.Equipment
{
    public interface IEquipable
    {
        public EquipmentContainer MainWeapon { get; set; }
        public EquipmentContainer SecondaryWeapon { get; set; }
        public EquipmentContainer TacticalItemInfo0 { get; set; }
        public EquipmentContainer TacticalItemInfo1 { get; set; }
        public EquipmentContainer TacticalItemInfo2 { get; set; }
        
        public List<EquipmentContainer> TacticalItemInfos { get; }

        public void InitEquipment(List<EquipmentConfig> equipments);
        // {
        //     TacticalItemInfo0,
        //     TacticalItemInfo1,
        //     TacticalItemInfo2
        // };
    }
}
using System.Collections.Generic;

namespace Systems.Equipment
{
    public interface IEquipable
    {
        public WeaponInfo MainWeapon { get; set; }
        public WeaponInfo SecondaryWeapon { get; set; }
        public TacticalItemInfo TacticalItemInfo0 { get; set; }
        public TacticalItemInfo TacticalItemInfo1 { get; set; }
        public TacticalItemInfo TacticalItemInfo2 { get; set; }
        
        public List<TacticalItemInfo> TacticalItemInfos { get; }
        // {
        //     TacticalItemInfo0,
        //     TacticalItemInfo1,
        //     TacticalItemInfo2
        // };
    }
}
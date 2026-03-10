using Systems.Equipment.Config;

namespace Systems.Equipment
{
    // 装备系统：
    // 单位可装备不同的各式武器装备，包括一把主武器、一把副武器和三个战术道具。主武器大多为枪械。战术道具包括：投掷类道具、医疗类道具、侦查类道具等。
    
    // 我不是很想封装地狱,先这样写着吧
    public abstract class EquipmentInfo
    {
        
    }
    
    public class WeaponInfo : EquipmentInfo
    {
        private WeaponConfig config;
    }
    
    public class TacticalItemInfo : EquipmentInfo
    {
        private TacticalItemConfig config;
    }
}
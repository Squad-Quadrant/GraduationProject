using System;
using Systems.Equipment.Config;

namespace Systems.Equipment
{
    public class EquipmentContainer
    {
        private EquipmentConfig _config;
        public EquipmentConfig Config => _config;
        
        private EquipmentLogic _logic;
        public EquipmentLogic Logic => _logic;

        public EquipmentType Type
        {
            get
            {
                if (_config == null) return EquipmentType.None;
                return _config.Type;
            }
        }
        
        public void Init(EquipmentConfig config)
        {
            _config = config;
            if (!config) return;
            switch (config.Type)
            {
                case EquipmentType.Weapon:
                    _logic = new WeaponLogic(config);
                    break;
                case EquipmentType.TacticalItem:
                    _logic = new TacticalItemLogic(config); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
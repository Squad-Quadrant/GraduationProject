using System;
using System.Collections.Generic;
using Systems.Equipment.Config;

namespace Systems.Equipment
{
    public interface IEquipmentService
    {
        public EquipmentConfig Get(int id);
        public EquipmentConfig Get(string name);
        
        public void Init();
    }
}

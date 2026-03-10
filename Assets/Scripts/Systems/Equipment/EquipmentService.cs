using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Equipment.Config;

namespace Systems.Equipment
{
    public class EquipmentService : IEquipmentService
    {
        private readonly Dictionary<int, EquipmentConfig> _data = new();

        public EquipmentConfig Get(int id)
        {
            return _data.GetValueOrDefault(id);
        }

        public EquipmentConfig Get(string name)
        {
            return _data.Values.FirstOrDefault(equipment => equipment.Name == name);
        }

        public void Init()
        {
            
        }
    }
}

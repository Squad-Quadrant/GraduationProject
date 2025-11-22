using System.Collections.Generic;
using System.Linq;

namespace Systems.Equipment
{
    public class EquipmentService : IEquipmentService
    {
        private readonly Dictionary<int, EquipmentData> _data = new();

        public EquipmentData Get(int id)
        {
            return _data.GetValueOrDefault(id);
        }

        public EquipmentData Get(string name)
        {
            return _data.Values.FirstOrDefault(equipment => equipment.Name == name);
        }

        public void Init()
        {
            
        }
    }
}
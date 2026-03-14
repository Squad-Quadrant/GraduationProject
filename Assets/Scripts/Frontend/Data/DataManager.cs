using System.Collections.Generic;
using PurpleFlowerCore.Utility;
using Systems.Equipment.Config;
using UnityEngine;

namespace Presentation.Data
{
    // 这是用于局内外数据管理和获取配置引用的类
    public class DataManager : DdolSingletonMono<DataManager>
    {
        [SerializeField] private List<EquipmentConfig> _equipmentConfigs;

        private Dictionary<string, int[]> _unitEquipmentData = new(); // 局外调DataManager填充数据

        private void Start()
        {
            // todo: 局外系统
            _unitEquipmentData.Add("001", new[] {1, 2, 0, 3, 4});
            _unitEquipmentData.Add("002", new[] {1, 2, 0, 3, 4});
            _unitEquipmentData.Add("003", new[] {1, 2, 0, 3, 4});
            // temp end
        }

        public List<EquipmentConfig> GetEquipmentConfigList(string unitID)
        {
            if (!_unitEquipmentData.TryGetValue(unitID, out var equipArray) || equipArray == null)
                return new List<EquipmentConfig> { null, null, null, null, null };

            var res = new List<EquipmentConfig>();
            foreach (var equipId in equipArray)
            {
                res.Add(_equipmentConfigs.Find(c => c.Id == equipId));
            }
            return res;
        }
        
    }
}
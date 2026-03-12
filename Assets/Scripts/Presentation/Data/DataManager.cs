using System;
using System.Collections.Generic;
using PurpleFlowerCore.Utility;
using Systems.Equipment.Config;
using UnityEngine;

namespace Presentation.Data
{
    // 这是用于局内外数据管理和获取配置引用的类
    public class DataManager : DdolSingletonMono<DataManager>, IDataManager
    {
        [SerializeField] private List<EquipmentConfig> weaponConfigList;

        private Dictionary<string, int[]> _unitEquipmentData = new(); // 局外调DataManager填充数据

        private void Start()
        {
            // todo: 局外系统
            _unitEquipmentData.Add("001", new[] {1, 2, 0, 3, 4});
            _unitEquipmentData.Add("002", new[] {1, 2, 0, 3, 4});
            _unitEquipmentData.Add("003", new[] {1, 2, 0, 3, 4});
            // temp end
        }

        
    }
}
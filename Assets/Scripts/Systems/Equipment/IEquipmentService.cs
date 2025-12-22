using System;
using System.Collections.Generic;

namespace Systems.Equipment
{
    // 主要用于管理装备配置数据，计划使用Excel转Json配置
    public interface IEquipmentService
    {
        [Obsolete("Obsolete")]
        public EquipmentData Get(int id);
        [Obsolete("Obsolete")]
        public EquipmentData Get(string name);
        
        // 从Json初始化所有装备数据
        public void Init();
    }
}

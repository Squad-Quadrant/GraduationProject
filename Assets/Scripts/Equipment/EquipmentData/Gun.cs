using PurpleFlowerCore;
using UnityEngine;

namespace Equipment
{
    [Configurable("Equipment/Gun")]
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "Data/Equipment Data/Gun")]
    public class Gun : EquipmentData
    {
        public int Atk;
        
    }
}
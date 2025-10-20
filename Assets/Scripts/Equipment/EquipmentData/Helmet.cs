using PurpleFlowerCore;
using UnityEngine;

namespace Equipment
{
    [Configurable("Equipment/Helmet")]
    [CreateAssetMenu(fileName = "EquipmentData", menuName = "Data/Equipment Data/Helmet")]
    public class Helmet : EquipmentData
    {
        public int Def;
    }
}
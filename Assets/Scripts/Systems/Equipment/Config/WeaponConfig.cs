using PurpleFlowerCore;
using UnityEngine;

namespace Systems.Equipment.Config
{
    [Configurable("Equipment/Weapon")]
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Game/Equipment/Weapon")]
    public class WeaponConfig : EquipmentConfig
    {
        public int AmmoCapacity;
    }
}
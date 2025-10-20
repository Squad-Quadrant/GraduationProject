using PurpleFlowerCore;
using UnityEditor;
using UnityEngine;

namespace Equipment
{
    [Configurable]
    public abstract class EquipmentData : ScriptableObject
    {
        public string Name;
    }
}
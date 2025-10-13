using PurpleFlowerCore;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "TestData", menuName = "TestData", order = 0)]
    [Configurable("Test")]
    public class TestData : ScriptableObject
    {
        public int test = 0;
    }
}
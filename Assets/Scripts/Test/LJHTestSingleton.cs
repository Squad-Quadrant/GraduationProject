using PurpleFlowerCore.Utility;
using UnityEngine;

namespace Test
{
    public class LJHTestSingleton : AutoSingletonMono<LJHTestSingleton>
    {
        public void DoSomething()
        {
            Debug.Log("Singleton Instance is working!");
        }
    }
}
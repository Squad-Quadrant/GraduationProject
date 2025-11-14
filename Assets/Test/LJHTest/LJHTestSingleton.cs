using PurpleFlowerCore.Utility;
using UnityEngine;

namespace Test.LJHTest
{
    public class LJHTestSingleton : AutoSingletonMono<LJHTestSingleton>
    {
        public void DoSomething()
        {
            Debug.Log("Singleton Instance is working!");
        }
    }
}

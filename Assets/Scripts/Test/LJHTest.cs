using Data;
using UnityEngine;

namespace Test
{
    public class LJHTest : MonoBehaviour
    {
        public int test = 0;
        public TestData data;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                test++;
                data.test++;
                LJHTestSingleton.Instance.DoSomething();
                Debug.Log("Unity怎么使的来着" + test + " " + data.test);
            }
            
        }
    }
}

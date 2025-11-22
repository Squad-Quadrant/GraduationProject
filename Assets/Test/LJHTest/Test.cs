using System;
using PurpleFlowerCore;
using UnityEngine;

namespace Test.LJHTest
{
    public class Test : MonoBehaviour
    {
        public void TEST()
        {
            PFCLog.Info(PFCConfig.WeaponData1.AK47.Damage);
        }
    }
}
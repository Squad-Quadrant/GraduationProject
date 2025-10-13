using System;
using UnityEngine;

namespace PurpleFlowerCore.Utility
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string Condition { get; }
        public ShowIfAttribute(string condition)
        {
            Condition = condition;
        }
    }
}
using System;

namespace PurpleFlowerCore.Utility
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct | AttributeTargets.Class)]
    public class InspectableAttribute : Attribute
    {
        public string Annotation { get; }
        public InspectableAttribute(string annotation = "")
        {
            Annotation = annotation;
        }
    }
}
using System;
using System.Reflection;
using PurpleFlowerCore.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PurpleFlowerCore.Editor.Tool
{
    [CustomEditor(typeof(ScriptableObject), true)]
    public class ScriptableObjectEditor : UnityEditor.Editor
    {
        private ScriptableObject Target => target as ScriptableObject;
        private Type Type => Target.GetType();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CheckAttribute();
        }
        
        private void CheckAttribute()
        {
            foreach (var method in Type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.GetCustomAttribute<BtnAttribute>() != null)
                {
                    if (GUILayout.Button(method.Name))
                    {
                        method.Invoke(Target, null);
                    }
                }
            }
        }
    }
}
using System.Reflection;
using PurpleFlowerCore.Utility;
using UnityEditor;
using UnityEngine;

namespace PurpleFlowerCore.Editor.Utility
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            var target = property.serializedObject.targetObject;
            MethodInfo method = target.GetType().GetMethod(showIf.Condition, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method != null && method.ReturnType == typeof(bool))
            {
                bool show = (bool)method.Invoke(target, null);

                if (show)
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            // else
            // {
            //     EditorGUI.LabelField(position, label.text, "Invalid condition method");
            // }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            Object target = property.serializedObject.targetObject;
            MethodInfo method = target.GetType().GetMethod(showIf.Condition, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        
            if (method != null && method.ReturnType == typeof(bool))
            {
                bool show = (bool)method.Invoke(target, null);
        
                if (show)
                {
                    return EditorGUI.GetPropertyHeight(property, label, true);
                }
            }

            return 0;
        }
    }
}
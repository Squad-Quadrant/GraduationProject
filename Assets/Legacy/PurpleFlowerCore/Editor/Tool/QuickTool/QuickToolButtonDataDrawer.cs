using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace PurpleFlowerCore.Editor.Tool
{
    [CustomPropertyDrawer(typeof(QuickToolButtonData))]
    public class QuickToolButtonDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            QuickToolButtonData target = property.managedReferenceValue as QuickToolButtonData;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUILayout.PropertyField(property.FindPropertyRelative("name"), new GUIContent("Name"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("lineBreak"), new GUIContent("Line Break"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("color"), new GUIContent("Color"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("commandType"), new GUIContent("Command Type"));
            if(target is { commandType: QuickToolButtonData.CommandType.Custom })
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("command"), new GUIContent("Command"));
            }
            else
                EditorGUILayout.PropertyField(property.FindPropertyRelative("commandParam"));

            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
    }
}
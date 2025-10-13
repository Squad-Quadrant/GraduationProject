using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace PurpleFlowerCore.Editor.Utility
{
    public static class UIUtility
    {
        // private void ShowField(FieldInfo field, object target)
        // {}
        public static void ShowField<T>(string label, ref object value, GUIStyle style = null, params GUILayoutOption[] options)
        {
            style ??= new GUIStyle();
            var type = typeof(T);
            if(type == typeof(int))
            {
                value = EditorGUILayout.IntField(label, (int)value, style, options);
            }
            else if (type == typeof(float))
            {
                value = EditorGUILayout.FloatField(label, (int)value, style, options);
            }
            else if (type == typeof(bool))
            {
                value = EditorGUILayout.Toggle(label, (bool)value, options);
            }
            else if (type == typeof(string))
            {
                value = EditorGUILayout.TextField(label, (string)value, style, options);
            }
            else if (type == typeof(Enum))
            {
                value = EditorGUILayout.EnumPopup(label, (Enum)value, style, options);
            }
            else if (type == typeof(Vector3))
            {
                value = EditorGUILayout.Vector3Field(label, (Vector3)value, options);
            }
            else if (type == typeof(Color))
                value = EditorGUILayout.ColorField(label, (Color)value, options);
            // else 
            //     value = ShowProperty()
                
        }

        public static void ShowField(FieldInfo info, object target, GUIStyle style = null, params GUILayoutOption[] options)
        {
            var type = info.FieldType;
            
            if(type == typeof(int))
            {
                info.SetValue(target,
                    style == null
                        ? EditorGUILayout.IntField(info.Name, (int)info.GetValue(target), options)
                        : EditorGUILayout.IntField(info.Name, (int)info.GetValue(target), style, options));
            }
            else if (type == typeof(float))
            {
                info.SetValue(target,
                    style == null
                        ? EditorGUILayout.FloatField(info.Name, (float)info.GetValue(target), options)
                        : EditorGUILayout.FloatField(info.Name, (float)info.GetValue(target), style, options));
            }
            else if (type == typeof(bool))
            {
                info.SetValue(target,
                    style == null
                        ? EditorGUILayout.Toggle(info.Name, (bool)info.GetValue(target), options)
                        : EditorGUILayout.Toggle(info.Name, (bool)info.GetValue(target), style, options));
            }
            else if (type == typeof(string))
            {
                info.SetValue(target,
                    style == null
                        ? EditorGUILayout.TextField(info.Name, (string)info.GetValue(target), options)
                        : EditorGUILayout.TextField(info.Name, (string)info.GetValue(target), style, options));
            }
            else if (type == typeof(Enum))
            {
                info.SetValue(target,
                    style == null
                        ? EditorGUILayout.EnumPopup(info.Name, (Enum)info.GetValue(target), options)
                        : EditorGUILayout.EnumPopup(info.Name, (Enum)info.GetValue(target), style, options));
            }
            else if (type == typeof(Vector3))
            {
                EditorGUILayout.Vector3Field(info.Name, (Vector3)info.GetValue(target), options);
            }
            else if (type == typeof(Color))
                EditorGUILayout.ColorField(info.Name, (Color)info.GetValue(target), options);
            else PropertyField(info, target, options);
        }
        
        /// <summary>
        /// 通用显示一个字段，当字段与List相关时，需要谨慎使用
        /// 具体体现为，当字段类型为List时，无法写入元素
        /// </summary>
        public static void PropertyField(FieldInfo info, object target, params GUILayoutOption[] options)
        {
            SerializedObject serializedObject = new SerializedObject((UnityEngine.Object)target);
            
            SerializedProperty property = serializedObject.FindProperty(info.Name);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(info.Name), true , options);
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                var style = new GUIStyle
                {
                    wordWrap = true,
                    normal =
                    {
                        textColor = Color.red
                    }
                };

                EditorGUILayout.LabelField(info.Name, $"Could not find or support Serialized Property : ({info.FieldType}){info.Name}",style);
            }
        }
        
        
        
        private static void ShowObject(ScriptableObject data, Type type = null)
        {
            var currentData = data;
            if (currentData == null) return;
            if (type == null)
            {
                if(currentData.GetType().BaseType != typeof(ScriptableObject))
                    ShowObject(data, currentData.GetType().BaseType);
            }
            else if(type.BaseType != typeof(ScriptableObject))
            {
                ShowObject(data, type.BaseType);
            }
            
            Type targetType = type ?? currentData.GetType();
            
            FieldInfo[] fields =
                targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            EditorGUI.BeginChangeCheck();
            
            foreach (FieldInfo field in fields)
            {
                if ((field.IsPublic && !Attribute.IsDefined(field, typeof(HideInInspector)))
                     || Attribute.IsDefined(field, typeof(SerializeField)))
                {
                    ShowField(field, currentData);
                }
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(currentData);
            }
        }

        private static void ShowField(FieldInfo field, object target)
        {
            Type fieldType = field.FieldType;
            object value = field.GetValue(target);

            if (fieldType == typeof(int))
            {
                field.SetValue(target, EditorGUILayout.IntField(field.Name, (int)value));
            }
            else if (fieldType == typeof(float))
            {
                field.SetValue(target, EditorGUILayout.FloatField(field.Name, (float)value));
            }
            else if (fieldType == typeof(string))
            {
                field.SetValue(target, EditorGUILayout.TextField(field.Name, (string)value));
            }
            else if (fieldType == typeof(bool))
            {
                field.SetValue(target, EditorGUILayout.Toggle(field.Name, (bool)value));
            }
            else if (fieldType.IsEnum)
            {
                field.SetValue(target, EditorGUILayout.EnumPopup(field.Name, (Enum)value));
            }
            else if (fieldType == typeof(Color))
            {
                field.SetValue(target, EditorGUILayout.ColorField(field.Name, (Color)value));
            }
            else if (fieldType == typeof(Vector3))
            {
                field.SetValue(target, EditorGUILayout.Vector3Field(field.Name, (Vector3)value));
            }
            else
            {
                UIUtility.PropertyField(field, target);
            }
        }
        
        // public static UnityEvent UnityEventField(string label, UnityEvent value, params GUILayoutOption[] options)
        // {
        //     SerializedObject serializedObject = new SerializedObject((UnityEngine.Object)target);
        //     SerializedProperty property = serializedObject.FindProperty(field.Name);
        //     if (property != null)
        //     {
        //         EditorGUILayout.PropertyField(property, new GUIContent(field.Name), true);
        //         serializedObject.ApplyModifiedProperties();
        //     }
        //     else
        //     {
        //         EditorGUILayout.LabelField(field.Name, "Could not find SerializedProperty for UnityEvent");
        //     }
        //     return value;
        // }
        // public static void ShowInt(string label, ref int value, GUIStyle style = null, params GUILayoutOption[] option)
        // {
        //     if(style == null)
        //         value = EditorGUILayout.IntField(label, value, option);
        //     else
        //         value = EditorGUILayout.IntField(label, value, style, option);
        // }
        //
        // public static void ShowFloat(string label, ref float value, GUIStyle style = null, params GUILayoutOption[] options)
        // {
        //     value = style == null ? EditorGUILayout.FloatField(label, value, options) : EditorGUILayout.FloatField(label, value, style, options);
        // }
        //
        // public static void ShowString(string label, ref string value, GUIStyle style = null, params GUILayoutOption[] options)
        // {
        //     if (style == null)
        //         value = EditorGUILayout.TextField(label, value, options);
        //     else
        //         value = EditorGUILayout.TextField(label, value, style, options);
        // }
        //
        // public static void ShowBool(string label, ref bool value, GUIStyle style = null, params GUILayoutOption[] options)
        // {
        //     if (style == null)
        //         value = EditorGUILayout.Toggle(label, value, options);
        //     else
        //         value = EditorGUILayout.Toggle(label, value, style, options);
        // }
        //
        // public static void ShowVector3(string label, ref Vector3 value, GUIStyle style = null, params GUILayoutOption[] options)
        // {
        //     if (style == null)
        //         value = EditorGUILayout.Vector3Field(label, value, options);
        //     else
        //     {
        //         EditorGUILayout.BeginHorizontal();
        //         EditorGUILayout.LabelField(label, style, options);
        //         value = EditorGUILayout.Vector3Field("", value, options);
        //         EditorGUILayout.EndHorizontal();
        //     }
        // }
        //
        // public static void ShowColor(string label, ref Color value, params GUILayoutOption[] options)
        // {
        //     value = EditorGUILayout.ColorField(label, value, options);
        // }
        //
        // public static void ShowEnum<T>(string label, ref T value, GUIStyle style = null, params GUILayoutOption[] options) where T : System.Enum
        // {
        //     if (style == null)
        //         value = (T)EditorGUILayout.EnumPopup(label, value, options);
        //     else
        //         value = (T)EditorGUILayout.EnumPopup(label, value, style, options);
        // }
        //
        // public static T ShowObject<T>(string label, T value, params GUILayoutOption[] options)
        // {
        //     value =EditorGUILayout.ObjectField(label, value, value.GetType(), false, options) as T;
        //     return value;
        // }
    }
    
    // public class TestWindow : EditorWindow
    // {
    //     private int _testValue = 0;
    //     
    //     [MenuItem("PFC/TestWindow")]
    //     public static void ShowWindow()
    //     {
    //         GetWindow<TestWindow>("TestWindow");
    //     }
    //
    //     private void OnGUI()
    //     {
    //         //EditorUI.ShowInt("Test Value", ref _testValue);
    //         PFCLog.Info("TestWindow" + _testValue);
    //         
    //     }
    // }
}

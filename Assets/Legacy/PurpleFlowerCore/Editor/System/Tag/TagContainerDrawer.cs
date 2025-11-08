using PurpleFlowerCore.Tag;
using UnityEditor;
using UnityEngine;

namespace PurpleFlowerCore.Tag
{
    [CustomPropertyDrawer(typeof(TagContainer))]
    public class TagContainerDrawer : PropertyDrawer
    {
        private GenericMenu dropDownMenu;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            var target = fieldInfo.GetValue(property.serializedObject.targetObject) as TagContainer;
            //dropDownMenu ??= GetGamePlayTagsMenu(property, target);
            if (target == null)
            {
                var style = new GUIStyle();
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("target is null", style);
                return;
            }
            try
            {
                foreach (var tag in target.GamePlayTags)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(tag);
                    if (GUILayout.Button("Remove"))
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Remove Tag");
                        target.RemoveTag(tag);
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            catch (System.Exception e)
            {
                if(e is not System.InvalidOperationException)
                    throw;
            }
            
            if (GUILayout.Button("Add"))
            {
                dropDownMenu.DropDown(new Rect(UnityEngine.Event.current.mousePosition, Vector2.zero));
            }
            property.serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space(5);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }

        // private GenericMenu GetGamePlayTagsMenu(SerializedProperty property, GamePlayTagContainer target)
        // {
        //     var rows = GameplayTagTable.Instance.GetAll();
        //     GenericMenu menu = new();
        //     
        //     foreach (var row in rows)
        //     {
        //         var tagValue = row.Value.Value;
        //         var tagPath = tagValue.Replace('.', '/');
        //         menu.AddItem(new GUIContent(tagPath as string), false, () =>
        //         {
        //             Undo.RecordObject( property.serializedObject.targetObject, "Add Tag");
        //             target.AddTag(tagValue);
        //             EditorUtility.SetDirty(property.serializedObject.targetObject);
        //         });
        //     }
        //     return menu;
        // }
    }
}

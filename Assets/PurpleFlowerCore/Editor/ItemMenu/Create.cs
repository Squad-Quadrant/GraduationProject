using PurpleFlowerCore.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace PurpleFlowerCore.Editor.ItemMenu
{
    public static class Create
    {
        private static EditorRef _editorRef;
        public static EditorRef EditorRef
        {
            get
            {
                if (_editorRef == null)
                    _editorRef = SOUtility.GetSOByType<EditorRef>(true);
                return _editorRef;
            }
        }
        [MenuItem("GameObject/PFC/PropertyBar")]
        public static void CreatePropertyBar()
        {
            CreatePrefab(EditorRef.PropertyBar);
        }
        
        private static void CreatePrefab(MonoBehaviour prefab)
        {
            var obj = GameObject.Instantiate(prefab);
            obj.name = prefab.name;
        }
    }
}
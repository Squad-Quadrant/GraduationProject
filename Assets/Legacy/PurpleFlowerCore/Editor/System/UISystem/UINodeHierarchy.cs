using UnityEditor;
using UnityEngine;
using System.Reflection;
using PurpleFlowerCore;
using PurpleFlowerCore.Editor;
using PurpleFlowerCore.Editor.Utility;

public class UINodeHierarchy
{
    private static EditorRef _ref;
    public static EditorRef Ref
    {
        get
        {
            if (_ref == null)
            {
                _ref = SOUtility.GetSOByType<EditorRef>();
            }
            return _ref;
        }
    }
    
    [InitializeOnLoadMethod]
    static void HierarchyExtensionIcon()
    {
        EditorApplication.hierarchyWindowItemOnGUI += (int instanceID, Rect selectionRect) =>
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;
            DrawIcon(go, selectionRect);
            DrawUITag(go,selectionRect);
        };
    }
    
    private static void DrawIcon(GameObject go, Rect selectionRect)
    {
        if (go.GetComponent<UINode>() != null)
        {
            Rect rect = new Rect(selectionRect);
            rect.x += rect.width - 10;
            GUI.Label(rect, Ref.Icon_Flower);
        }
    }

    private static void DrawUITag(GameObject go, Rect selectionRect)
    {
        var uiNode = go.GetComponentInParent<UINode>(true);
        if (uiNode)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = uiNode.TagColor;
            style.fontSize = 10;
            Rect rect = new Rect(selectionRect);
            // if (uiNode.name == null)
            // {
            //     uiNode.name = "";
            // }
            rect.x += rect.width - 15 - uiNode.name.Length * 5;
            GUI.Label(rect, uiNode.name,style);
        }
    }
}

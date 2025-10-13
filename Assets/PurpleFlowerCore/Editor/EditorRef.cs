using PurpleFlowerCore.Component;
using UnityEngine;

namespace PurpleFlowerCore.Editor
{
    //在没有资源管理系统情况下的笨办法
    public class EditorRef : ScriptableObject
    {
        public PropertyBar PropertyBar;
        public Texture Icon_Flower;
        public Texture Icon_Head;
    }
}
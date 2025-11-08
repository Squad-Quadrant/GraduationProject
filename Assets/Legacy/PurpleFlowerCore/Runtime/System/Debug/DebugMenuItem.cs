using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PurpleFlowerCore.PFCDebug
{
    public class DebugMenuItem : MonoBehaviour
    {
        [SerializeField] private Text commandName;
        [SerializeField] private Button commandButton;
        
        public void Init(TreeNode<CommandBase> commandNode, DebugMenu debugMenu)
        {
            commandButton.onClick.RemoveAllListeners();
            string text = commandNode.name;
            if(commandNode.IsLeaf && commandNode.Value.ParamType != null)
            {
                text += " " + commandNode.Value.ParamType.Name;
            }
            else if (!commandNode.IsLeaf)
            {
                text += ">";
            }
            commandName.text = text;
            commandButton.onClick.AddListener(() => debugMenu.ClickItem(commandNode));
        }
    }
}
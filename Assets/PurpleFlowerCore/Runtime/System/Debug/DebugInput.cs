using UnityEngine;
using UnityEngine.UI;

namespace PurpleFlowerCore.PFCDebug
{
    public class DebugInput : MonoBehaviour
    {
        [SerializeField]private InputField inputField;
        [SerializeField]private DebugMenu debugMenu;
        public void OnSubmit(string input)
        {
            inputField.text = "";
            debugMenu.Input(input);
        }

        public void Change(string input)
        {
            
        }
        
        public void Show(TreeNode<CommandBase> commandNode)
        {
            inputField.text = commandNode.name + " ";
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
}
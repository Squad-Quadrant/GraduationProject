using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace PurpleFlowerCore.PFCDebug
{
    public class Prompt : MonoBehaviour
    {
        [SerializeField] private Text text;
        public void ShowStackTrace(StackFrame[] stackFrames)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var stackFrame in stackFrames)
            {
                sb.Append(stackFrame.GetMethod().DeclaringType);
                sb.Append(".");
                sb.Append(stackFrame.GetMethod().Name);
                sb.Append(" (");
                sb.Append(stackFrame.GetFileName());
                sb.Append(":");
                sb.Append(stackFrame.GetFileLineNumber());
                sb.Append(")\n");
            }
            text.text = sb.ToString();
            text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
        }
    }
}
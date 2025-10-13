using UnityEngine;
using UnityEngine.UI;

namespace PurpleFlowerCore.PFCDebug
{
    public class DebugLevelButton : MonoBehaviour
    {
        private bool _isOpen = true;
        [SerializeField] private Image image;
        [SerializeField] private LogLevel level;
        [SerializeField] private LogPanel logPanel;

        public void Chick()
        {
            _isOpen = !_isOpen;
            float alpha = _isOpen ? 1 : 0.5f;
            var color = image.color;
            color.a = alpha;
            image.color = color;
            logPanel.SwitchLevel(level,_isOpen);
        }
        
        public void SetOpen(bool isOpen)
        {
            _isOpen = isOpen;
            float alpha = _isOpen ? 1 : 0.5f;
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
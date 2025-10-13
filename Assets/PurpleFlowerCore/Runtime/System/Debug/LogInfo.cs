using System.Text;
using PurpleFlowerCore.PFCDebug;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using PurpleFlowerCore.Utility;

namespace PurpleFlowerCore
{
    public class LogInfo : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Text text;
        [SerializeField] private Image background;
        private Prompt _prompt;

        private LogData _data;

        public LogData Data
        {
            set
            {
                _data = value;
                SetText();
            }
            get => _data;
        }

        public void Init(LogData data, Prompt prompt)
        {
            _prompt = prompt;
            _data = data;
            SetText();
        }
        
        private void SetText()
        {
            // var prefixColor = DebugSystem.GetLogLevelColor(_data.Level);
            // var prefix = $"<color=#{ColorUtility.ToHtmlStringRGB(prefixColor)}>[{_data.Level}]</color>";
            // var channel = _data.Channel == null ? "" : $"[{_data.Channel}]";
            // var time = _data.Time == null ? "" : $"[<size=20>{_data.Time}]</size>";
            // var content = $"<color=#{ColorUtility.ToHtmlStringRGB(_data.Color)}>{_data.Content}</color>";
            //
            // var sb = new StringBuilder();
            // sb.Append(time);
            // sb.Append(prefix);
            // sb.Append(channel);
            // sb.Append(content);

            text.text = Format();
            text.rectTransform.sizeDelta = new Vector2(text.rectTransform.sizeDelta.x, text.preferredHeight);
        }

        public string Format()
        {
            var prefixColor = DebugSystem.GetLogLevelColor(_data.Level);
            var prefix = $"<color=#{ColorUtility.ToHtmlStringRGB(prefixColor)}>[{_data.Level}]</color>";
            var channel = _data.Channel == null ? "" : $"[{_data.Channel}]";
            var time = _data.Time == null ? "" : $"[<size=20>{_data.Time}</size>]";
            var content = $"<color=#{ColorUtility.ToHtmlStringRGB(_data.Color)}>{_data.Content}</color>";
            var sb = new StringBuilder();
            sb.Append(time);
            sb.Append(prefix);
            sb.Append(channel);
            sb.Append(content);
            return sb.ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            FadeUtility.FadeOut(background,100);
            if(_prompt == null || _data.StackFrames == null || _data.StackFrames.Length == 0) return;
            _prompt.ShowStackTrace(_data.StackFrames);
        }
    }
}
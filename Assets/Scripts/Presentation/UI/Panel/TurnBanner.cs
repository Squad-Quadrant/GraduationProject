using Presentation.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    public class TurnBanner : UIPanel
    {
        [SerializeField] private Text text;
        public void Show(string context)
        {
            text.text = context;
        }

        // public void Initialize(TurnEndedEvent data)
        // {
        //     text.text = $"下一单位: {data.TurnNumber}";
        // }
    }
}
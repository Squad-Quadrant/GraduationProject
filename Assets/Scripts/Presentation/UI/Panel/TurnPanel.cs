using Data.Runtime.Events.Turn;
using Presentation.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    // 这是一个临时的UI面板,用来测试nextUnit,优先保证游戏能动
    public class TurnPanel : UIPanel
    {
        [SerializeField] private Button nextUnitButton;
        [SerializeField] private Button nextTurnButton;
        
        public void NextUnit()
        {
            nextUnitButton.onClick.AddListener(() => EventBus.Publish(new UnitTurnEndedEvent()));
        }

        public void NextTurn()
        {
            nextTurnButton.onClick.AddListener(() => EventBus.Publish(new TurnEndedEvent()));
        }
    }
}
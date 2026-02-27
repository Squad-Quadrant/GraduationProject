using System;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.View;
using Presentation.UI.Core;
using PurpleFlowerCore.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI.Panel
{
    // 这是一个临时的UI面板,用来测试nextUnit,优先保证游戏能动
    public class TurnPanel : UIPanel
    {
        [SerializeField] private Button nextUnitButton;
        [SerializeField] private Button nextTurnButton;

        protected override void OnInitialize()
        {
            nextUnitButton.onClick.AddListener(() => { 
                EventBus.Publish(new UnitTurnEndedEvent());
                DelayUtility.Delay(1f, () =>
                {
                    EventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.UI, PresentationType.UI.TurnBanner));
                });
            });
            nextTurnButton.onClick.AddListener(() => EventBus.Publish(new TurnEndedEvent()));

        }
    }
}
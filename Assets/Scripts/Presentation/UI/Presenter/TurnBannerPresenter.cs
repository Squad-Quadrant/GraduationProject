using System;
using Core.Events;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.View;
using Presentation.UI.Core;
using Presentation.UI.Panel;
using PurpleFlowerCore.Utility;

namespace Presentation.UI.Presenter
{
    public class TurnBannerPresenter : IDisposable
    {
        private readonly UIManager _uiManager;
        private readonly IEventBus _eventBus;
        private TurnBanner _turnBanner;

        public TurnBannerPresenter(UIManager uiManager, IEventBus eventBus)
        {
            _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<TurnStartedEvent>(ShowActionMenu);
            _eventBus.Subscribe<UnitTurnStartedEvent>(ShowActionMenu);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<TurnStartedEvent>(ShowActionMenu);
            _eventBus.Unsubscribe<UnitTurnStartedEvent>(ShowActionMenu);
        }

        private void ShowActionMenu(TurnStartedEvent e)
        {
            // _turnBanner = _uiManager.Open<TurnBanner>();
            // _turnBanner.Show($"下一回合: {e.TurnNumber}");
            // DelayUtility.Delay(2f, () =>
            // {
            //     _eventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.UI, PresentationType.UI.TurnBanner));
            //     _uiManager.Close(_turnBanner);
            // });
        }
        
        private void ShowActionMenu(UnitTurnStartedEvent e)
        {
            // _turnBanner = _uiManager.Open<TurnBanner>();
            //     _turnBanner.Show($"下一单位: {e.UnitId}");
            // DelayUtility.Delay(2f, () =>
            // {
            //     _eventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.UI, PresentationType.UI.TurnBanner));
            //     _uiManager.Close(_turnBanner);
            // });
        }
    }
}

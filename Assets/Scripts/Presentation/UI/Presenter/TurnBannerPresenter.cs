using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Turn;
using Data.Runtime.Events.View;
using Presentation.UI.Core;
using Presentation.UI.Panel;
using PurpleFlowerCore.Utility;
using Systems.Unit;

namespace Presentation.UI.Presenter
{
    public class TurnBannerPresenter : IDisposable
    {
        private readonly UIManager _uiManager;
        private readonly IEventBus _eventBus;
        private readonly IUnitService _unitService;
        private TurnBannerPanel _bannerPanel;

        public TurnBannerPresenter(UIManager uiManager, IEventBus eventBus, IUnitService unitService)
        {
            _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));

            _eventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            _eventBus.Subscribe<UnitTurnStartedEvent>(OnUnitTurnStarted);
            _eventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);

            this.Log("Initialized");
        }

        public void Dispose()
        {
	        _eventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
	        _eventBus.Unsubscribe<UnitTurnStartedEvent>(OnUnitTurnStarted);
	        _eventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
        }

        private void OnTurnStarted(TurnStartedEvent e)
        {
	        this.Log($"Turn {e.TurnNumber} started — showing banner");

	        ShowBanner(
		        title: $"Round {e.TurnNumber}",
		        subtitle: null,
		        presentationType: PresentationType.UI.TurnStart
	        );
        }

        private void OnUnitTurnStarted(UnitTurnStartedEvent e)
        {
	        this.Log($"Unit '{e.UnitId}' turn started — showing transition");

	        ShowBanner(
		        title: "Next Unit",
		        subtitle: _unitService.TryGetUnit(e.UnitId, out var unit) ? unit.name : null,
		        presentationType: PresentationType.UI.UnitTransition
	        );
        }

        private void OnTurnEnded(TurnEndedEvent e)
        {
	        this.Log($"Turn {e.TurnNumber} ended — showing banner");

	        ShowBanner(
		        title: $"Round {e.TurnNumber} Complete",
		        subtitle: null,
		        presentationType: PresentationType.UI.TurnEnd
	        );
        }

        private void ShowBanner(string title, string subtitle, string presentationType)
        {
	        _bannerPanel = _uiManager.Open<TurnBannerPanel>();

	        if (!_bannerPanel)
	        {
		        this.LogWarning("Failed to open TurnBanner — completing immediately");
		        _eventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.UI, presentationType));
		        return;
	        }

	        _bannerPanel.Show(title, subtitle, () =>
	        {
		        _uiManager.Close(_bannerPanel);
		        _bannerPanel = null;
		        _eventBus.Publish(new PresentationCompleteEvent(EPresentationCategory.UI, presentationType));
	        });
        }
    }
}

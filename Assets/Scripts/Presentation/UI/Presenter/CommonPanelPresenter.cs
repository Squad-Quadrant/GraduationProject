using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events;
using Presentation.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel;
using Systems.Interfaces;
using Systems.Unit;

namespace Presentation.UI.Presenter
{
    public class CommonPanelPresenter : IDisposable
    {
        private readonly UIManager _uiManager;
        private readonly IEventBus _eventBus;
        private readonly ICoordinateConverter  _coordinateConverter;
        private readonly IUnitService _unitService;
        private BloodSliderPanel _bloodSliderPanel = new();
        
        public CommonPanelPresenter(UIManager uiManager, IEventBus eventBus, ICoordinateConverter coordinateConverter, IUnitService unitService)
        {
            _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _coordinateConverter = coordinateConverter ?? throw new ArgumentNullException(nameof(coordinateConverter));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            
            _eventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);

            this.Log("Initialized");
        }
        
        public void Dispose()
        {

        }

        private void OnLevelLoaded(LevelLoadedEvent e)
        {
            _bloodSliderPanel = _uiManager.Open<BloodSliderPanel>();
            _bloodSliderPanel.Init(_eventBus, _coordinateConverter, _unitService);
        }

        private void OnGameOver()
        {
            
        }
    }
}
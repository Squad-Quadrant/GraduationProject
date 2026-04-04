using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events;
using Presentation.Interaction;
using Presentation.UI.Core;
using Presentation.UI.Panel;
using Presentation.UI.Panel.Log;
using Presentation.Unit;
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
        private readonly UnitViewManager _unitViewManager;
        
        private BloodSliderPanel _bloodSliderPanel;
        private DamageTextPanel _damageTextPanel;
        private GameLogger _gameLogger;
        
        public CommonPanelPresenter(UIManager uiManager, IEventBus eventBus, ICoordinateConverter coordinateConverter,
            IUnitService unitService, UnitViewManager unitViewManager)
        {
            _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _coordinateConverter = coordinateConverter ?? throw new ArgumentNullException(nameof(coordinateConverter));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _unitViewManager =  unitViewManager ?? throw new ArgumentNullException(nameof(unitViewManager));
            _eventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);

            this.Log("Initialized");
        }
        
        public void Dispose()
        {

        }

        private void OnLevelLoaded(LevelLoadedEvent e)
        {
            _bloodSliderPanel = _uiManager.Open<BloodSliderPanel>();
            _bloodSliderPanel.Init(_coordinateConverter, _unitService, _unitViewManager);
            
            _damageTextPanel  = _uiManager.Open<DamageTextPanel>();
            _damageTextPanel.Init(_coordinateConverter, _unitService);
            _gameLogger = _uiManager.Open<GameLogger>();
            
        }

        private void OnGameOver()
        {
            
        }
    }
}
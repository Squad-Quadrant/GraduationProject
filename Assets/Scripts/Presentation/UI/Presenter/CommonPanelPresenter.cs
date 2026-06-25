using System;
using Core.Events;
using Core.Log;
using Data.Runtime.Events;
using Data.Runtime.Events.Damage;
using Presentation.UI.Core;
using Presentation.UI.Panel;
using Presentation.UI.Panel.Blood;
using Presentation.UI.Panel.BodyPartPrompt;
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
        private BodyPartPromptPanel _bodyPartPromptPanel;
        
        public CommonPanelPresenter(
	        UIManager uiManager,
	        IEventBus eventBus,
	        ICoordinateConverter coordinateConverter,
            IUnitService unitService,
	        UnitViewManager unitViewManager)
        {
	        _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
	        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
	        _coordinateConverter = coordinateConverter ?? throw new ArgumentNullException(nameof(coordinateConverter));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _unitViewManager =  unitViewManager ?? throw new ArgumentNullException(nameof(unitViewManager));

            eventBus.Subscribe<LevelLoadedEvent>(OnLevelLoaded);
            this.Log("Initialized");
        }

        public void Dispose()
        {
	        _eventBus.Unsubscribe<LevelLoadedEvent>(OnLevelLoaded);

	        if (_bloodSliderPanel) _uiManager.Close(_bloodSliderPanel);
	        if (_damageTextPanel) _uiManager.Close(_damageTextPanel);
	        if (_gameLogger) _uiManager.Close(_gameLogger);
	        if (_bodyPartPromptPanel) _uiManager.Close(_bodyPartPromptPanel);

	        _bloodSliderPanel = null;
	        _damageTextPanel = null;
	        _gameLogger = null;
	        _bodyPartPromptPanel = null;
        }

        private void OnLevelLoaded(LevelLoadedEvent e)
        {
            _bloodSliderPanel = _uiManager.Open<BloodSliderPanel>();
            _bloodSliderPanel.Init(_coordinateConverter, _unitService, _unitViewManager);
            
            _damageTextPanel  = _uiManager.Open<DamageTextPanel>();
            _damageTextPanel.Init(_coordinateConverter, _unitService);
            
            _gameLogger = _uiManager.Open<GameLogger>();
            
            _bodyPartPromptPanel = _uiManager.Open<BodyPartPromptPanel>();
        }
        

        private void OnGameOver()
        {
            
        }
    }
}

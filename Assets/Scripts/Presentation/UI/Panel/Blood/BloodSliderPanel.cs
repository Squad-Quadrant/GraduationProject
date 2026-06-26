using System.Collections.Generic;
using Data.Runtime.Events.Unit;
using Presentation.UI.Core;
using Presentation.Unit;
using Systems.Interfaces;
using Systems.Unit;
using UnityEngine;

namespace Presentation.UI.Panel.Blood
{
    public class BloodSliderPanel : UIPanel
    {
        private ICoordinateConverter  _coordinateConverter;
        private IUnitService _unitService;
        private UnitViewManager _unitViewManager;
        [SerializeField] private BloodSlider bloodSliderPrototype;
        private readonly Dictionary<Systems.Unit.Unit ,BloodSlider> _bloodSliders = new();
        
        public void Init(ICoordinateConverter coordinateConverter, IUnitService unitService, UnitViewManager unitViewManager)
        {
            _coordinateConverter = coordinateConverter;
            _unitService = unitService;
            _unitViewManager = unitViewManager;

            var allUnits = _unitService.GetAllUnits();
            foreach (var unit in allUnits) CreateUnitBloodSlider(unit);
        }

        protected override void OnOpen()
        {
	        EventBus.Subscribe<UnitCreatedEvent>(OnUnitCreated);
	        EventBus.Subscribe<UnitDestroyedEvent>(OnUnitDestroyed);
        }

        protected override void OnClose()
        {
	        EventBus.Unsubscribe<UnitCreatedEvent>(OnUnitCreated);
	        EventBus.Unsubscribe<UnitDestroyedEvent>(OnUnitDestroyed);

	        foreach (var slider in _bloodSliders.Values) Destroy(slider.gameObject);
	        _bloodSliders.Clear();
        }
        
        private void OnUnitCreated(UnitCreatedEvent e) => CreateUnitBloodSlider(e.Unit);

        private void CreateUnitBloodSlider(Systems.Unit.Unit unit)
        {
	        if (_bloodSliders.ContainsKey(unit)) return;

            var slider = Instantiate(bloodSliderPrototype, transform);
            slider.Init(unit, _coordinateConverter, _unitViewManager.GetView(unit.id), EventBus);
            _bloodSliders.Add(unit, slider);
        }
        
        private void OnUnitDestroyed(UnitDestroyedEvent e) => OnUnitDestroyed(e.Unit);

        private void OnUnitDestroyed(Systems.Unit.Unit unit)
        {
	        if (!_bloodSliders.Remove(unit, out var slider)) return;
	        Destroy(slider.gameObject);
        }
    }
}

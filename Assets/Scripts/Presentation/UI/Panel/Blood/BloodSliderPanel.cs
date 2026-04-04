using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Unit;
using Presentation.UI.Core;
using Presentation.Unit;
using Systems.Interfaces;
using Systems.Unit;
using UnityEngine;

namespace Presentation.UI.Panel
{
    public class BloodSliderPanel : UIPanel
    {
        private ICoordinateConverter  _coordinateConverter;
        private IUnitService _unitService;
        private UnitViewManager _unitViewManager;
        [SerializeField] private BloodSlider bloodSliderPrototype;
        private Dictionary<Systems.Unit.Unit ,BloodSlider> _bloodSliders = new();
        
        
        protected override void OnInitialize()
        {
            this.Log("OnInitialize");
        }
        
        public void Init(ICoordinateConverter coordinateConverter, IUnitService unitService, UnitViewManager unitViewManager)
        {
            _coordinateConverter = coordinateConverter;
            _unitService = unitService;
            _unitViewManager = unitViewManager;
            EventBus.Subscribe<UnitCreatedEvent>(OnUnitCreated);
            EventBus.Subscribe<UnitDestroyedEvent>(OnUnitDestroyed);

            var allUnits = _unitService.GetAllUnits();
            foreach (var unit in allUnits)
            {
                OnUnitCreated(unit);
            }
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<UnitCreatedEvent>(OnUnitCreated);
            EventBus.Unsubscribe<UnitDestroyedEvent>(OnUnitDestroyed);
            base.OnDestroy();
        }
        
        private void OnUnitCreated(UnitCreatedEvent e)
        {
            OnUnitCreated(e.Unit);
        }

        private void OnUnitCreated(Systems.Unit.Unit unit)
        {
            var slider = Instantiate(bloodSliderPrototype, transform);
            slider.Init(unit, _coordinateConverter, _unitViewManager.GetView(unit.id), EventBus);
            _bloodSliders.Add(unit, slider);
        }
        
        private void OnUnitDestroyed(UnitDestroyedEvent e)
        {
            OnUnitDestroyed(e.Unit);
        }

        private void OnUnitDestroyed(Systems.Unit.Unit unit)
        {
            var slider = _bloodSliders[unit];
            _bloodSliders.Remove(unit);
            Destroy(slider.gameObject);
        }
    }
}
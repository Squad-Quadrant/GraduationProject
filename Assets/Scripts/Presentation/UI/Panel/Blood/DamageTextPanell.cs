using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Unit;
using Presentation.UI.Core;
using Systems.Interfaces;
using Systems.Unit;
using UnityEngine;

namespace Presentation.UI.Panel
{
    public class DamageTextPanell : UIPanel, IDisposable
    {
        private IEventBus _eventBus;
        private ICoordinateConverter  _coordinateConverter;
        private IUnitService _unitService;
        [SerializeField] private DamageText damageTextPrototype;
        
        
        protected override void OnInitialize()
        {
            this.Log("OnInitialize");
        }
        
        public void Init(IEventBus eventBus, ICoordinateConverter coordinateConverter, IUnitService unitService)
        {
            _eventBus = eventBus;
            _coordinateConverter = coordinateConverter;
            _unitService = unitService;
            _eventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        private void OnDamageApplied(DamageAppliedEvent e)
        {
            var damageText = Instantiate(damageTextPrototype, transform);
            damageText.Init(e.Context, _coordinateConverter);
        }
    }
}
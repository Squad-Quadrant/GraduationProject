using System;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Damage;
using Presentation.UI.Core;
using Systems.Interfaces;
using Systems.Unit;
using UnityEngine;

namespace Presentation.UI.Panel
{
    public class DamageTextPanel : UIPanel
    {
        private ICoordinateConverter  _coordinateConverter;
        private IUnitService _unitService;
        [SerializeField] private DamageText damageTextPrototype;
        [SerializeField] private float interval = 0.1f;
        
        private readonly Queue<DamageAppliedEvent> _damageEventQueue = new();
        private bool _isProcessingDamageEvents;
        
        protected override void OnInitialize()
        {
            this.Log("OnInitialize");
        }
        
        public void Init(ICoordinateConverter coordinateConverter, IUnitService unitService)
        {
            _coordinateConverter = coordinateConverter;
            _unitService = unitService;
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            base.OnDestroy();
        }

        private void OnDamageApplied(DamageAppliedEvent e)
        {
            // 制造e的浅拷贝
            
            _damageEventQueue.Enqueue(e);
            if (!_isProcessingDamageEvents)
            {
                StartCoroutine(ProcessDamageEvents());
            }
        }

        private IEnumerator ProcessDamageEvents()
        {
            _isProcessingDamageEvents = true;
            while (_damageEventQueue.Count > 0)
            {
                var e = _damageEventQueue.Dequeue();
                var damageText = Instantiate(damageTextPrototype, transform);
                damageText.Init(e.Context, _coordinateConverter);
                yield return new WaitForSeconds(interval);
            }
            _isProcessingDamageEvents = false;
        }
    }
}
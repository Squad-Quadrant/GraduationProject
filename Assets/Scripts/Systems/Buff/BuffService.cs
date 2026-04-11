using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data;
using Data.Runtime.Events.Turn;
using Systems.Buff.Config;
using Systems.Unit;
using Object = UnityEngine.Object;

namespace Systems.Buff
{
    public class BuffService : IBuffService, IDisposable
    {
        private Dictionary<IBuffAble, BuffProxy> _buffProxies = new();
        public Dictionary<IBuffAble, BuffProxy> BuffProxies => _buffProxies;
        
        private readonly IEventBus _eventBus;
        private readonly DataManager _dataManager;
        private readonly UnitService  _unitService;
        private int _buffCount;

        public BuffService(IEventBus eventBus, DataManager dataManager, UnitService unitService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            this.Log("Initialized");
            
            _eventBus.Subscribe<UnitTurnStartedEvent>(OnUnitTurnStarted);
        }
        
        public void Dispose()
        {
            _eventBus.Unsubscribe<UnitTurnStartedEvent>(OnUnitTurnStarted);
        }
        
        public void Register(BuffProxy buffProxy)
        {
            IBuffAble target = buffProxy.Owner;
            if (_buffProxies.ContainsKey(target))
            {
                this.LogError($"BuffService already has a BuffProxy for target {target}");
                return;
            }
            _buffProxies.Add(target, buffProxy);
        }

        public BuffInfo CreateBuffInfo(BuffType type, IBuffAble target, Object creator)
        {
            var data = _dataManager.GetBuffData(type);
            if (data == null)
            {
                this.LogError($"BuffService.CreateBuffInfo: buffType {type} not exist");
                return null;
            }
            
            _buffCount++;
            BuffInfo info = new(data, creator, target, _buffCount);
            return info;
        }
    
        // public void AttachBuff(BuffType buffType, IBuffAble target, Object creator)
        // {
        //     
        // }
        //
        // public void LostBuff(BuffInfo buffInfo)
        // {
        //     
        // }
    
        public void ResetBuff()
        {
            foreach (var buff in _buffProxies)
            {
                buff.Value.Reset();
            }
        }

        private void OnUnitTurnStarted(UnitTurnStartedEvent e)
        {
            var unit = _unitService.GetUnit(e.UnitId);
            if (_buffProxies.ContainsKey(unit))
            {
                _buffProxies[unit].Turn();
            }
            else
            {
                this.LogError($"BuffService.OnUnitTurnStarted: no BuffProxy found for unit {unit}");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Log;
using Data;
using Data.Runtime.Events.Turn;
using Systems.Buff.Config;
using Object = UnityEngine.Object;

namespace Systems.Buff
{
    public class BuffService : IBuffService, IDisposable
    {
        private Dictionary<IBuffAble, BuffProxy> _buffProxies = new();
        public Dictionary<IBuffAble, BuffProxy> BuffProxies => _buffProxies;
        
        private readonly IEventBus _eventBus;
        private readonly DataManager _dataManager;
        // private readonly UnitService  _unitService;
        private int _buffCount;

        public BuffService(IEventBus eventBus, DataManager dataManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            // _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            this.Log("Initialized");
            
            _eventBus.Subscribe<UnitTurnEffectsResolvingEvent>(OnUnitTurnEffectsResolving);
            // _eventBus.Subscribe<UnitCreatedEvent>(OnUnitCreated);
            // _eventBus.Subscribe<UnitDestroyedEvent>(ONUnitDestroyed);
        }
        
        public void Dispose()
        {
	        _eventBus.Unsubscribe<UnitTurnEffectsResolvingEvent>(OnUnitTurnEffectsResolving);
            // _eventBus.Unsubscribe<UnitCreatedEvent>(OnUnitCreated);
            // _eventBus.Unsubscribe<UnitDestroyedEvent>(ONUnitDestroyed);
        }

        public void Register(IBuffAble target)
        {
            if (_buffProxies.ContainsKey(target))
            {
                this.LogError($"BuffService already has a BuffProxy for target {target}");
                return;
            }
            var proxy = new BuffProxy(this, target, _eventBus);
            target.BuffProxy = proxy;
            _buffProxies.Add(target, proxy);
        }

        public void Unregister(IBuffAble target)
        {
            if (!_buffProxies.ContainsKey(target))
            {
                this.LogError($"BuffService does not have a BuffProxy for target {target}");
                return;
            }
            _buffProxies.Remove(target);
        }

        public BuffInfo CreateBuffInfo(BuffType type, IBuffAble target, object creator)
        {
            var data = _dataManager.GetBuffData(type);
            if (data == null)
            {
                this.LogError($"BuffService.CreateBuffInfo: buffType {type} not exist");
                return null;
            }
            
            _buffCount++;
            BuffInfo info = new(data, creator, target, _eventBus, _buffCount);
            return info;
        }
    
        public void ResetBuff()
        {
            foreach (var buff in _buffProxies)
            {
                buff.Value.Reset();
            }
        }

        private void OnUnitTurnEffectsResolving(UnitTurnEffectsResolvingEvent e)
        {
	        var unit = _buffProxies.Keys.FirstOrDefault(u => (u as Unit.Unit)?.id == e.TurnUnitId);
	        if (unit == null) return;    // 无 proxy 就静默跳过
	        _buffProxies[unit].Turn();
        }
    }
}

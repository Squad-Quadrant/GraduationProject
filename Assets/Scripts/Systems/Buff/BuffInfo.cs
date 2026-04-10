using System;
using Systems.Buff.Config;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.Buff
{
    public enum BuffType
    {
        Fracture,
        Toxicosis,
        Blind
    }
    
    [Serializable]
    public class BuffInfo
    {
        private BuffData _buffData;
        private Object _creator;
        private IBuffAble _target;
        private int _durationCounter;
        private int _currentStack;
        private int _uid; // 运行时生成的id
        
        public BuffData BuffData => _buffData;
        public Object Creator => _creator;
        public IBuffAble Target => _target;
        public float DurationCounter => _durationCounter;
        public int CurrentStack => _currentStack;
        public int Uid => _uid;
        
        public BuffType BuffType => BuffData.buffType;
        public BuffAttachType AttachType => BuffData.attachType;
        public BuffLostType LostType => BuffData.lostType;
        public int MaxStack => BuffData.maxStack;
        public int DurationTurn => BuffData.durationTurn;

        public BuffInfo(BuffData buffData, Object creator, IBuffAble target, int uid)
        {
            _buffData = buffData;
            _creator = creator;
            _target = target;
            _uid = uid;
        }

        public bool Mergeable()
        {
            return AttachType != BuffAttachType.Keep;
        }

        // public void OnInit()
        // {
        //     if (_buffData.onInitEvents is null) return;
        //     foreach (var onInitEvent in _buffData.onInitEvents)
        //     {
        //         onInitEvent.Trigger(this);
        //     }
        // }

        public void Merge(int stackNum)
        {
            if (AttachType == BuffAttachType.Add)
            {
                _currentStack += stackNum;
                _currentStack = _currentStack > MaxStack ? MaxStack : _currentStack;
            }

            if (AttachType == BuffAttachType.Override)
            {
                _durationCounter = 0;
            }
            
            OnAttach();
        }
        
        public void OnAttach()
        {
            if (_buffData.onAttachEvents is null) return;
            foreach (var onAttachEvent in _buffData.onAttachEvents)
            {
                onAttachEvent.Trigger(this);
            }
        }
        
        public void OnLost()
        {
            if (_buffData.onLostEvents is null) return;
            foreach (var onLostEvent in _buffData.onLostEvents)
            {
                onLostEvent.Trigger(this);
            }
        }
        
        public void OnTurn()
        {
            if (_buffData.onTurnEvents is null) return;
            foreach (var onTurnEvent in _buffData.onTurnEvents)
            {
                onTurnEvent.Trigger(this);
            }
        }

        public void OnReset()
        {
            if (_buffData.onResetEvents is null) return;
            foreach (var onResetEvent in _buffData.onResetEvents)
            {
                onResetEvent.Trigger(this);
            }
        }

        public void OnProperty<T>(PropertyType propertyType, ref T baseValue) where T : struct, IConvertible
        {
            if (_buffData.onTurnEvents is null) return;
            foreach (var onTurnEvent in _buffData.propertyInfluences)
            {
                if (onTurnEvent.propertyType == propertyType)
                    onTurnEvent.Execute(this, ref baseValue);
            }
        }
    }
}

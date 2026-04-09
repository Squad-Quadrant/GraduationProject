using System;
using Systems.Buff.Config;
using UnityEngine;

namespace Systems.Buff
{
    [Serializable]
    public class BuffInfo : IComparable<BuffInfo>
    {
        private BuffData _buffData;
        private MonoBehaviour _creator;
        private IBuffAble _target;
        private float _durationCounter;
        private float _tickCounter;
        private int _currentStack;
        private int _id;
        private int _uid; // 运行时生成的id
        
        public BuffData BuffData => _buffData;
        public MonoBehaviour Creator => _creator;
        public IBuffAble Target => _target;
        public float DurationCounter => _durationCounter;
        public float TickCounter => _tickCounter;
        public int CurrentStack => _currentStack;
        public int Id => _id;
        public int Uid => _uid;

        public BuffInfo(BuffData buffData, MonoBehaviour creator, Unit.Unit target)
        {
            this._buffData = buffData;
            this._creator = creator;
            this._target = target;
        }

        public int CompareTo(BuffInfo other)
        {
            // if (ReferenceEquals(this, other)) return 0;
            //
            // if (other == null) return 1;
            // if (buffData == null) return other.buffData == null ? 0 : -1;
            // if (other.buffData == null) return 1;
            //
            // if (target == other.target)
            //     return buffData.id.CompareTo(other.buffData.id);
            // else return target.ID > other.target.ID ? 1 : -1;
            // todo: 适配我们游戏的Buff规则
            return 1;
        }

        // public void OnInit()
        // {
        //     if (_buffData.onInitEvents is null) return;
        //     foreach (var onInitEvent in _buffData.onInitEvents)
        //     {
        //         onInitEvent.Trigger(this);
        //     }
        // }
        
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

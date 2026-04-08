using System;
using Systems.Buff.Config;
using UnityEngine;

namespace Systems.Buff
{
    public enum BuffType
    {
        UnitBuff,
        CellBuff
    }
    
    [Serializable]
    public class BuffInfo : IComparable<BuffInfo>
    {
        private BuffData buffData;
        private GameObject creator;
        private Unit.Unit target;
        private float durationCounter;
        private float tickCounter;
        private int currentStack;
        private int id; // 运行时生成的id
        
        public BuffData BuffData => buffData;
        public GameObject Creator => creator;
        public Unit.Unit Target => target;
        public float DurationCounter => durationCounter;
        public float TickCounter => tickCounter;
        public int CurrentStack => currentStack;
        public int Id => id;

        public BuffInfo(BuffData buffData, GameObject creator, Unit.Unit target)
        {
            this.buffData = buffData;
            this.creator = creator;
            this.target = target;
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

        public void OnInit()
        {
            if (buffData.onInitEvents is null) return;
            foreach (var onInitEvent in buffData.onInitEvents)
            {
                onInitEvent.Trigger(this);
            }
        }
        
        public void OnAttach()
        {
            if (buffData.onAttachEvents is null) return;
            foreach (var onAttachEvent in buffData.onAttachEvents)
            {
                onAttachEvent.Trigger(this);
            }
        }
        
        public void OnLost()
        {
            if (buffData.onLostEvents is null) return;
            foreach (var onLostEvent in buffData.onLostEvents)
            {
                onLostEvent.Trigger(this);
            }
        }
        
        public void OnTurn()
        {
            if (buffData.onTurnEvents is null) return;
            foreach (var onTurnEvent in buffData.onTurnEvents)
            {
                onTurnEvent.Trigger(this);
            }
        }

        public void OnReset()
        {
            if (buffData.onResetEvents is null) return;
            foreach (var onResetEvent in buffData.onResetEvents)
            {
                onResetEvent.Trigger(this);
            }
        }
    }
}

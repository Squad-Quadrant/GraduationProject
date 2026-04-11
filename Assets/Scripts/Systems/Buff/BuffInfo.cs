using System;
using Systems.Buff.Config;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Systems.Buff
{
    [Serializable]
    public class BuffInfo
    {
        [SerializeField]private BuffData buffData;
        [SerializeField]private Object creator;
        [SerializeField]private int durationCounter;
        [SerializeField]private int currentStack;
        [SerializeField]private int uid; // 运行时生成的id
        private IBuffAble _target;
        
        public string Name => BuffData.name;
        public BuffData BuffData => buffData;
        public Object Creator => creator;
        public IBuffAble Target => _target;
        public float DurationCounter => durationCounter;
        public int CurrentStack => currentStack;
        public int Uid => uid;
        
        public BuffType BuffType => BuffData.buffType;
        public BuffAttachType AttachType => BuffData.attachType;
        public BuffLostType LostType => BuffData.lostType;
        public int MaxStack => BuffData.maxStack;
        public int DurationTurn => BuffData.durationTurn;

        public BuffInfo(BuffData buffData, Object creator, IBuffAble target, int uid)
        {
            this.buffData = buffData;
            this.creator = creator;
            _target = target;
            this.uid = uid;
        }

        public bool Mergeable()
        {
            return AttachType != BuffAttachType.Keep;
        }

        public void Merge(int stackNum)
        {
            if (AttachType == BuffAttachType.Add)
            {
                currentStack += stackNum;
                currentStack = currentStack > MaxStack ? MaxStack : currentStack;
            }

            if (AttachType == BuffAttachType.Override)
            {
                durationCounter = 0;
            }
            
            OnAttach();
        }
        
        public void OnAttach()
        {
            if (currentStack <= 0)
            {
                currentStack = 1;
            }
            
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
            durationCounter++;
            if (durationCounter >= DurationTurn)
            {
                durationCounter = 0;
                if (LostType == BuffLostType.Reduce)
                {
                    currentStack--;
                }
                else if (LostType == BuffLostType.Clear)
                {
                    currentStack = 0;
                }
            }
            
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

        public void OnProperty(BuffProperty property)
        {
            if (buffData.onTurnEvents is null) return;
            foreach (var onTurnEvent in buffData.propertyInfluences)
            {
                if (onTurnEvent.propertyType == property.Type)
                    onTurnEvent.Execute(this, property);
            }
        }
    }
}

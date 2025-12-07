using Systems.Buff;
using UnityEngine;

namespace Data.Config.Buff
{
    public abstract class BuffEvent : ScriptableObject
    {
        public abstract void Trigger(BuffInfo buffInfo);
    }
}
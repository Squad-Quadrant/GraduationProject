using UnityEngine;

namespace Systems.Buff.Config
{
    public abstract class BuffEvent : ScriptableObject
    {
        public abstract void Trigger(BuffInfo buffInfo);
    }
}

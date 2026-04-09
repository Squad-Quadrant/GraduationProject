using System;

namespace Systems.Buff.Config
{
    public abstract class BuffInfluence
    {
        public PropertyType propertyType;

        public abstract void Execute<T>(BuffInfo buffInfo,ref T baseValue) where T : struct, IConvertible;
    }
}
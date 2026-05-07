using System.Collections.Generic;
using UnityEngine;

namespace Systems.Buff.Config
{
    [CreateAssetMenu(fileName = "RemoveAllBuffEvent", menuName = "Game/Buff/BuffEvent/RemoveAllBuffEvent")]
    public class RemoveAllBuffEvent : UnitBuffEvent
    {
        public bool onlyRemoveShowInUI = true;
        protected override void Trigger(BuffInfo buffInfo, Unit.Unit unit)
        {
            var buffsToRemove = new List<BuffInfo>();
            foreach (var buff in unit.BuffProxy.BuffInfos)
            {
                if (!onlyRemoveShowInUI || buff.BuffData.showInUI)
                {
                    buffsToRemove.Add(buff);
                }
            }

            foreach (var buff in buffsToRemove)
            {
                unit.BuffProxy.Lost(buff);
            }
        }
    }
}
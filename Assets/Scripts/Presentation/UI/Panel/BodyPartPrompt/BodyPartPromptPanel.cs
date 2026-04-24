using Data.Runtime.Events.Damage;
using Presentation.UI.Core;
using Systems.Damage;
using UnityEngine;

namespace Presentation.UI.Panel.BodyPartPrompt
{
    public class BodyPartPromptPanel : UIPanel
    {
        [SerializeField] private BodyPartPrompt prompt;
        
        protected override void OnOpen()
        {
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        protected override void OnClose()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        private void OnDamageApplied(DamageAppliedEvent e)
        {
            var context = e.Context;
            if (!context.IsFinalCalculated)
                return;
            if (context.isMiss || context.DamageType != DamageType.Bullet || context.bodyPartType == BodyPartType.None)
                return;
            
            prompt.Hit(context.bodyPartType, context.TotalDamage > 70, context.FinalCalculatedNum);
        }
    }
}
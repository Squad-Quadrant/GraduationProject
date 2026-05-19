using System.Collections.Generic;
using Core.Commands;
using DG.Tweening;
using Systems.Interaction;
using Systems.Interaction.Targeting;
using UnityEngine;

namespace Systems.Unit.Skill.Logic
{
    public abstract class InstantlyUseSkillLogic : SkillLogic, ITargeted
    {
        public InstantlyUseSkillLogic(SkillConfig config, Unit owner) : base(config, owner)
        {
        }

        public IReadOnlyList<Vector2Int> GetValidCells(InteractionContext ctx)
        {
            return new[] { Owner.position };
        }

        public bool ValidateTarget(Vector2Int cell, InteractionContext ctx)
        {
            return cell == Owner.position;
        }

        public IReadOnlyList<Vector2Int> GetAreaEffectPreview(Vector2Int hoverCell)
        {
            return new[] { hoverCell };
        }

        public ICommand CreateCommand(Vector2Int target, InteractionContext ctx)
        {
            return 
            new AsyncLambdaCommand(
                $"{Owner.name} Use {Name}",
                onComplete =>
                {
                    Owner.CurrentAp -= ApCost;
                    Consume();

                    Use();
                    
                    DOVirtual.DelayedCall(0.2f, () => onComplete());
                });
        }

        public abstract void Use();
    }
}
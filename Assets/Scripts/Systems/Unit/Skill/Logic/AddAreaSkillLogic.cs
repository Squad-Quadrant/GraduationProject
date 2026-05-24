using System;
using System.Collections.Generic;
using Core.Commands;
using Core.Log;
using Data.Runtime.Events.Skill;
using DG.Tweening;
using Presentation.Bootstrap;
using Systems.AreaEffect;
using Systems.Interaction;
using Systems.Interaction.Targeting;
using Systems.PathFinding;
using UnityEngine;

namespace Systems.Unit.Skill.Logic
{
	public abstract class AddAreaSkillLogic : SkillLogic, ITargeted
    {
        protected PathFindingOptions PathFindingOptions;
        private IPathFindingService _pathFindingService;
        protected IPathFindingService PathFindingService => _pathFindingService ??= LevelContainer.Instance.Resolve<IPathFindingService>();

        protected AddAreaSkillConfig AddAreaConfig;
        private static readonly Vector2Int[] BfsDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
		public AddAreaSkillLogic(SkillConfig config, Unit owner) : base(config, owner)
		{
            AddAreaConfig = config as AddAreaSkillConfig;
            PathFindingOptions = new PathFindingOptions(
                canPassThroughAllies: true,
                enemiesBlockMovement: false,
                movingUnitFaction: Owner.faction,
                movingUnitId: Owner.id,
                canCrossHighWalls: false,
                canCrossLowWalls: false,
                ignoreTerrainWalkability: true);
		}
        
        public virtual bool ValidateTarget(Vector2Int cell, InteractionContext ctx) =>
            ctx.VisionCalculator.TraceRay(Owner.position, cell, out _);

        public IReadOnlyList<Vector2Int> GetAreaEffectPreview(Vector2Int hoverCell) =>
            ExpandCoverage(hoverCell);

        public IReadOnlyList<Vector2Int> GetValidCells(InteractionContext ctx)
        {
            return new[] { Owner.position };
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

                        Use(ctx);
                        EventBus.Publish(new SkillUsedEvent(Owner, this));
                        DOVirtual.DelayedCall(0.2f, () => onComplete());
                    });
        }
        
        public abstract void Use(InteractionContext ctx);
        
        protected List<Vector2Int> ExpandCoverage(Vector2Int center)
        {
            var offsets = AddAreaConfig.coverageOffsets;
            if (offsets == null || offsets.Length == 0)
                return new List<Vector2Int> { center };

            var candidates = new HashSet<Vector2Int>(offsets.Length);
            foreach (var offset in offsets)
                candidates.Add(center + offset);

            var result = new List<Vector2Int>(offsets.Length);
            var visited = new HashSet<Vector2Int> { center };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(center);

            if (candidates.Contains(center))
                result.Add(center);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var direction in BfsDirections)
                {
                    var next = current + direction;
                    if (!candidates.Contains(next)) continue;
                    if (!visited.Add(next)) continue;
                    if (!PathFindingService.CanTraverseBetween(current, next, PathFindingOptions)) continue;
                    queue.Enqueue(next);
                    result.Add(next);
                }
            }
            return result;
        }
        
        protected void BuildAreaEffect(
            Vector2Int target,
            AreaEffectBehavior behavior,
            InteractionContext ctx)
        {
            var cells = ExpandCoverage(target);
            
            // if (AddAreaConfig.clip) AudioService.PlaySfx(AddAreaConfig.clipWhenLanded);

            var effect = ctx.AreaEffectService.Register(
                ownerId:        Owner.id,
                targetCell:     target,
                cells:          cells,
                remainingTurns: AddAreaConfig.persistTurns,
                behavior:       behavior);

            this.Log($"Registered {effect}");
            
        }

    }
}

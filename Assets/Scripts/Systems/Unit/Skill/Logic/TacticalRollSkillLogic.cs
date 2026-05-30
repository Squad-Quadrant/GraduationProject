using System.Collections.Generic;
using Core.Commands;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Skill;
using Presentation.Bootstrap;
using Systems.Interaction;
using Systems.Map;
using Systems.PathFinding;
using UnityEngine;

namespace Systems.Unit.Skill.Logic
{
    public class TacticalRollSkillLogic : InstantlyUseSkillLogic
    {
        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        private TacticalRollSkillConfig RollConfig => Config as TacticalRollSkillConfig;

        private IMapService _mapService;
        private IMapService MapService => _mapService ??= LevelContainer.Instance.Resolve<IMapService>();

        private IPathFindingService _pathFindingService;
        private IPathFindingService PathFindingService => _pathFindingService ??= LevelContainer.Instance.Resolve<IPathFindingService>();

        private PathFindingOptions RollPathOptions => new(
            canPassThroughAllies: false,
            enemiesBlockMovement: true,
            movingUnitFaction: Owner.faction,
            movingUnitId: Owner.id,
            canCrossLowWalls: false,
            canCrossHighWalls: false,
            ignoreTerrainWalkability: false);

        public TacticalRollSkillLogic(SkillConfig config, Unit owner) : base(config, owner)
        {
            
        }

        public override IReadOnlyList<Vector2Int> GetValidCells(InteractionContext ctx)
        {
            var result = new List<Vector2Int>(Directions.Length);
            var distance = GetDistance();
            if (distance <= 0) return result;

            foreach (var direction in Directions)
            {
                for (var step = 1; step <= distance; step++)
                {
                    var target = Owner.position + direction * step;
                    if (TryBuildRollPath(target, out _))
                        result.Add(target);
                }
            }

            return result;
        }

        public override bool ValidateTarget(Vector2Int cell, InteractionContext ctx)
            => TryBuildRollPath(cell, out _);

        public override IReadOnlyList<Vector2Int> GetAreaEffectPreview(Vector2Int hoverCell)
            => new[] { hoverCell };

        public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx)
        {
            return new AsyncLambdaCommand(
                $"{Owner.name} Use {Name}",
                onComplete =>
                {
                    if (!TryBuildRollPath(target, out var path))
                    {
                        this.LogWarning($"Cannot tactical roll from {Owner.position} to {target}: invalid target or blocked path.");
                        onComplete?.Invoke();
                        return;
                    }

                    var moveCommand = new MoveUnitCommand(
                        Owner.id,
                        Owner.position,
                        target,
                        path,
                        ApCost,
                        ctx.UnitService,
                        ctx.MapService,
                        ctx.EventBus)
                    {
                        CountAsMovementSpend = false,
                        AnimationSpeedMultiplier = GetAnimationSpeedMultiplier()
                    };

                    moveCommand.Execute(() =>
                    {
                        Consume();
                        EventBus.Publish(new SkillUsedEvent(Owner, this));
                        onComplete?.Invoke();
                    });
                });
        }

        public override void Use()
        {
            this.LogWarning("TacticalRollSkillLogic requires a target cell. Use CreateCommand(target, ctx) instead.");
        }

        private bool TryBuildRollPath(Vector2Int target, out IReadOnlyList<Vector2Int> path)
        {
            path = null;

            var distance = GetDistance();
            if (distance <= 0) return false;

            var delta = target - Owner.position;
            var rollDistance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
            if (rollDistance <= 0 || rollDistance > distance) return false;

            var isHorizontal = delta.y == 0 && Mathf.Abs(delta.x) == rollDistance;
            var isVertical = delta.x == 0 && Mathf.Abs(delta.y) == rollDistance;
            if (!isHorizontal && !isVertical) return false;

            var direction = new Vector2Int(
                delta.x == 0 ? 0 : delta.x / Mathf.Abs(delta.x),
                delta.y == 0 ? 0 : delta.y / Mathf.Abs(delta.y));

            var cells = new List<Vector2Int>(rollDistance + 1) { Owner.position };
            var current = Owner.position;
            var options = RollPathOptions;

            for (var i = 1; i <= rollDistance; i++)
            {
                var next = Owner.position + direction * i;
                if (!MapService.Data.IsInBounds(next)) return false;
                if (!PathFindingService.CanTraverseBetween(current, next, options)) return false;

                cells.Add(next);
                current = next;
            }

            path = cells;
            return true;
        }

        private int GetDistance() => Mathf.Max(0, RollConfig?.distance ?? 0);

        private float GetAnimationSpeedMultiplier() => Mathf.Max(0.01f, RollConfig?.animationSpeedMultiplier ?? 1f);
    }
}
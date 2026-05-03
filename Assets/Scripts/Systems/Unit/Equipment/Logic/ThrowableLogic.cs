using System.Collections.Generic;
using Core.Commands;
using Core.Log;
using Data.Runtime.Commands;
using Data.Runtime.Events.Damage;
using Data.Runtime.Events.Vfx;
using Systems.AreaEffect;
using Systems.AreaEffect.Behaviors;
using Systems.Damage;
using Systems.Interaction;
using Systems.Interaction.Targeting;
using Systems.Unit.Equipment.Config;
using Systems.Vision;
using UnityEngine;

namespace Systems.Unit.Equipment.Logic
{
	// 投掷类战术道具的基类 Logic
	public abstract class ThrowableLogic : TacticalItemLogic, ITargeted
	{
		protected ThrowableLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public virtual IReadOnlyList<Vector2Int> GetValidCells(InteractionContext ctx)
		{
			var result = new List<Vector2Int>();
			var origin = Owner.position;
			int r = ItemConfig.throwRange;

			// 遍历曼哈顿距离 ≤ r 的所有格子
			for (int dx = -r; dx <= r; dx++)
			{
				int maxDy = r - Mathf.Abs(dx);
				for (int dy = -maxDy; dy <= maxDy; dy++)
				{
					var cell = origin + new Vector2Int(dx, dy);
					if (!ctx.MapService.Data.IsInBounds(cell)) continue;

					if (!ctx.VisionCalculator.TraceRay(origin, cell, out _)) continue;

					result.Add(cell);
				}
			}
			return result;
		}

		public virtual bool ValidateTarget(Vector2Int cell, InteractionContext ctx) =>
			ctx.VisionCalculator.TraceRay(Owner.position, cell, out _);

		public IReadOnlyList<Vector2Int> GetAreaEffectPreview(Vector2Int hoverCell) =>
			ExpandCoverage(hoverCell);

		public abstract ICommand CreateCommand(Vector2Int target, InteractionContext ctx);

		protected ICommand BuildAreaEffectCommand(
			Vector2Int target,
			AreaEffectBehavior behavior,
			InteractionContext ctx)
		{
			var cells = ExpandCoverage(target);

			return new ThrowCommand(
				owner: Owner,
				targetCell: target,
				projectilePrefab: ItemConfig.projectilePrefab,
				eventBus: ctx.EventBus,
				onLaunched: () =>
				{
					Owner.CurrentAp -= ItemConfig.apCost;
					Consume();
				},
				onLanded: () =>
				{
					var effect = ctx.AreaEffectService.Register(
						ownerId:        Owner.id,
						targetCell:     target,
						cells:          cells,
						remainingTurns: ItemConfig.persistTurns,
						behavior:       behavior);

					this.Log($"Registered {effect}");
				});
		}
        
        public override int GetDamage()
        {
            return ItemConfig.directDamage;
        }
	}

	// 手雷
	public class ThrowableGrenadeLogic : ThrowableLogic, IDamageInfluencer
	{
		public ThrowableGrenadeLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx)
		{
			var aoeCells = ExpandCoverage(target);
			var damage = ItemConfig.directDamage;

			return new ThrowCommand(
				owner: Owner,
				targetCell: target,
				projectilePrefab: ItemConfig.projectilePrefab,
				eventBus: ctx.EventBus,
				onLaunched: () =>
				{
					Owner.CurrentAp -= ItemConfig.apCost;
					Consume();
				},
				onLanded: () =>
				{
					ctx.EventBus.PublishOneShotVfx(ItemConfig.oneShotVfxPrefab, target);

					foreach (var cell in aoeCells)
					{
						var unit = ctx.UnitService.GetUnitAtPosition(cell);
						if (unit is not { IsAlive: true }) continue;
						if (GetDamage() <= 0) continue;
                        
                        var info = new GeneralDamageTriggeringInfo(this, unit);
                        ctx.EventBus.Publish(new DealDamageEvent(info));
					}
				});
		}

        public string DisplayName => Name();
        public List<DamageInfluence> GetDamageInfluences(DamageExecutingContext context)
        {
            return new List<DamageInfluence>
            {
                new GeneralHPInfluence(1, GetDamage(), this)
            };
        }
    }

	// 燃烧弹
	public class ThrowableBurnLogic : ThrowableLogic
	{
		public ThrowableBurnLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
			BuildAreaEffectCommand(
				target,
				new AttachBuffAreaBehavior(
					buffType:    ItemConfig.appliedBuff,
					displayName: ItemConfig.nName,
					displayIcon: ItemConfig.icon,
					persistentVfxPrefab: ItemConfig.persistentVfxPrefab),
				ctx);
	}

	// 定时炸弹
	public class ThrowableTimerBombLogic : ThrowableLogic
	{
		public ThrowableTimerBombLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
			BuildAreaEffectCommand(
				target,
				new CountdownExplosionBehavior(
					damage:      ItemConfig.directDamage,
					displayName: ItemConfig.nName,
					displayIcon: ItemConfig.icon,
					persistentVfxPrefab: ItemConfig.persistentVfxPrefab,
					explosionVfxPrefab: ItemConfig.oneShotVfxPrefab),
				ctx);
	}

	// 侦察眼
	public class ThrowableScoutEyeLogic : ThrowableLogic
	{
		public ThrowableScoutEyeLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

		public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
			BuildAreaEffectCommand(
				target,
				new ScoutEyeBehavior(
					visionRadius: ItemConfig.visionReach,
					displayName:  ItemConfig.nName,
					displayIcon:  ItemConfig.icon,
					persistentVfxPrefab: ItemConfig.persistentVfxPrefab),
				ctx);
	}
    
    // todo: 当该类道具数量增多,抽取GeneralThrowableLogic,将不同的AreaEffectBehavior作为参数传入
    // 照明弹
    public class ThrowableLightLogic : ThrowableLogic
    {
        public ThrowableLightLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

        public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
            BuildAreaEffectCommand(
                target,
                new LightBehavior(
                    displayName:  ItemConfig.nName,
                    displayIcon:  ItemConfig.icon,
                    persistentVfxPrefab: ItemConfig.persistentVfxPrefab),
                ctx);
    }
    
    // 烟雾弹
    public class ThrowableSmokeLogic : ThrowableLogic
    {
        public ThrowableSmokeLogic(TacticalItemConfig config, Unit owner) : base(config, owner) { }

        public override ICommand CreateCommand(Vector2Int target, InteractionContext ctx) =>
            BuildAreaEffectCommand(
                target,
                new SmokeBehavior(
                    displayName:  ItemConfig.nName,
                    displayIcon:  ItemConfig.icon,
                    persistentVfxPrefab: ItemConfig.persistentVfxPrefab),
                ctx);
    }
}

// using System;
// using Core.Commands;
// using Core.Log;
// using DG.Tweening;
// using Systems.Interaction;
// using Systems.Interaction.Targeting;
// using UnityEngine;
//
// namespace Systems.Unit.Skill.Logic
// {
// 	public class InstantHealLogic : SkillLogic, IInstantUsable
// 	{
// 		public InstantHealLogic(SkillConfig config, Unit owner) : base(config, owner)
// 		{
// 			// if (config.kind != ESkillKind.InstantHeal)
// 			// 	throw new ArgumentException($"SkillConfig kind mismatch: expected InstantHeal, got {config.kind}", nameof(config));
// 		}
//
// 		public ICommand CreateCommand(InteractionContext ctx) =>
// 			new AsyncLambdaCommand(
// 				$"Skill/InstantHeal({Owner.name} +{Config.healAmount}HP)",
// 				onComplete =>
// 				{
// 					Owner.CurrentAp -= Config.apCost;
// 					Consume();
//
// 					int newHp = Mathf.Min(Owner.CurrentHp + Config.healAmount, Owner.maxHp);
// 					int actualHeal = newHp - Owner.CurrentHp;
// 					Owner.CurrentHp = newHp;
//
// 					this.Log($"{Owner.name} healed {actualHeal} HP → {Owner.CurrentHp}/{Owner.maxHp}");
//
// 					DOVirtual.DelayedCall(0.2f, () => onComplete()); // todo: 需要动画或者反馈
// 				});
//
// 	}
// }

using System;
using System.Collections.Generic;
using Core.Events;
using Core.Log;
using Data.Runtime;
using Data.Runtime.Events.Damage;
using DG.Tweening;
using Systems.Vision;
using UnityEngine;

namespace Presentation.Vision
{
	public class GunLineRevealPresenter : IDisposable
	{
		private readonly IEventBus _eventBus;
		private readonly IVisionCalculator _visionCalculator;
		private readonly IVisionService _visionService;
		private readonly float _revealDuration;

		private readonly Dictionary<int, Tween> _activeReveals = new();

		public GunLineRevealPresenter(
			IEventBus eventBus,
			IVisionCalculator visionCalculator,
			IVisionService visionService,
			float revealDuration = 2f)
		{
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_visionCalculator = visionCalculator ?? throw new ArgumentNullException(nameof(visionCalculator));
			_visionService = visionService ?? throw new ArgumentNullException(nameof(visionService));
			_revealDuration = revealDuration;

			_eventBus.Subscribe<UnitAttackedDealDamageEvent>(OnAttackDealDamage);

			this.Log("Initialized");
		}

		public void Dispose()
		{
			_eventBus.Unsubscribe<UnitAttackedDealDamageEvent>(OnAttackDealDamage);

			// Kill all pending tweens and remove reveals from VisionService
			foreach (var (tokenId, tween) in _activeReveals)
			{
				tween?.Kill();
				_visionService.RemoveTemporaryReveal(new RevealToken(tokenId));
			}
			_activeReveals.Clear();
		}

		private void OnAttackDealDamage(UnitAttackedDealDamageEvent e)
		{
			if (e.Attacker == null || e.Target == null || e.ActionType != EActionType.Attack) return;

			var from = e.Attacker.position;
			var to = e.Target.position;

			var cells = new List<Vector2Int>();
			_visionCalculator.TraceRay(from, to, cells);

			if (cells.Count == 0) return;

			var token = _visionService.AddTemporaryReveal(cells);
			this.Log($"Gun-line reveal: {from} → {to}, {cells.Count} cells, duration={_revealDuration}s");

			var tween = DOVirtual.DelayedCall(_revealDuration, () =>
			{
				_visionService.RemoveTemporaryReveal(token);
				_activeReveals.Remove(token.Id);
			});

			_activeReveals[token.Id] = tween;
		}
	}
}

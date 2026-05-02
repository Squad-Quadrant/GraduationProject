using System;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.Vfx;
using Data.Runtime.Events.View;
using Systems.Unit;
using UnityEngine;

namespace Data.Runtime.Commands
{
	public class ThrowCommand : AsyncCommand
	{
		private readonly Unit _owner;
		private readonly Vector2Int _targetCell;
		private readonly GameObject _projectilePrefab;
		private readonly Action _onLaunched; // 投掷开始时立即执行（扣 AP / Consume）
		private readonly Action _onLanded;   // 投掷物落地时执行（VFX / AreaEffect / 伤害）

		private readonly IEventBus _eventBus;

		private Action<PresentationCompleteEvent> _onPresentationComplete;

		public override string Name => $"Throw({_owner.name} → {_targetCell})";
		public override bool CanUndo => false;

		public ThrowCommand(
			Unit owner,
			Vector2Int targetCell,
			GameObject projectilePrefab,
			IEventBus eventBus,
			Action onLaunched = null,
			Action onLanded = null)
		{
			_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			_targetCell = targetCell;
			_projectilePrefab = projectilePrefab;
			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
			_onLaunched = onLaunched;
			_onLanded = onLanded;
		}

		protected override void OnExecuteAsync()
		{
			this.Log($"Executing: {Name}");
			_onLaunched?.Invoke();

			_onPresentationComplete = OnPresentationComplete;
			_eventBus.Subscribe(_onPresentationComplete);

			_eventBus.Publish(new ThrowEvent(_owner.id, _targetCell, _projectilePrefab));
		}

		private void OnPresentationComplete(PresentationCompleteEvent e)
		{
			if (!e.Matches(EPresentationCategory.Animation, PresentationType.Animation.Throw, _owner.id))
				return;

			this.Log($"Throw landed: {Name}");

			_onLanded?.Invoke();

			Cleanup();
			CompleteExecution();
		}

		private void Cleanup()
		{
			if (_onPresentationComplete == null) return;
			_eventBus.Unsubscribe(_onPresentationComplete);
			_onPresentationComplete = null;
		}
	}
}

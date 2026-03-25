using System;
using System.Collections.Generic;
using System.Linq;
using Core.Commands;
using Core.Events;
using Core.Log;
using Data.Runtime.Events.View;
using Presentation.Bootstrap;

namespace Data.Runtime.Commands
{
	public class AwaitPresentationCommand : AsyncCommand
	{
		private readonly EPresentationCategory? _category;
		private readonly Action _onComplete;

		private readonly IEventBus _eventBus;
		private readonly List<Condition> _conditions = new();
		private readonly HashSet<string> _pending = new();

		private Action<PresentationCompleteEvent> _handler;
		private bool _completed;

		public override string Name => _conditions.Count == 1
			? $"AwaitPresentation({_conditions[0]})"
			: $"AwaitPresentation({_conditions.Count} conditions)";

		public override bool CanUndo => false;

		public AwaitPresentationCommand(Action onComplete = null)
		{
			_eventBus = RootContainer.Instance.Resolve<IEventBus>();
			_onComplete = onComplete;
		}

		public AwaitPresentationCommand Expect(
			EPresentationCategory category,
			string type,
			string entityId = null,
			string tag = null)
		{
			_conditions.Add(new Condition(category, type, entityId, tag));
			return this;
		}

		protected override void OnExecuteAsync()
		{
			if (_conditions.Count == 0)
			{
				this.Log("No conditions specified, completing immediately");
				CompleteExecution();
				return;
			}

			_completed = false;

			foreach (var cond in _conditions)
				_pending.Add(cond.Key);

			this.Log($"Waiting for: {string.Join(", ", _pending)}");

			_handler = OnPresentationComplete;
			_eventBus.Subscribe(_handler);
		}

		private void OnPresentationComplete(PresentationCompleteEvent e)
		{
			if (_completed) return;

			this.Log($"Received matching event: {e}");

			foreach (var cond in _conditions.Where(cond => e.Matches(cond.Category, cond.Type, cond.EntityId, cond.Tag)))
			{
				_pending.Remove(cond.Key);
				this.Log($"Received: {cond.Key}, remaining: {_pending.Count}");
				if (_pending.Count != 0) continue;

				this.Log("All conditions met");
				Complete();
			}
		}

		private void Complete()
		{
			CompleteExecution();
			_completed = true;
			_onComplete?.Invoke();
			Cleanup();
		}

		private void Cleanup()
		{
			if (_handler == null) return;
			_eventBus.Unsubscribe(_handler);
			_handler = null;
		}

		public void ForceComplete()
		{
			if (_completed) return;
			this.LogWarning($"Skipping wait, {_pending.Count} conditions remaining");
			Complete();
		}

		private readonly struct Condition
		{
			public readonly EPresentationCategory Category;
			public readonly string Type;
			public readonly string EntityId;
			public readonly string Tag;
			public readonly string Key;

			public Condition(
				EPresentationCategory category,
				string type,
				string entityId,
				string tag)
			{
				Category = category;
				Type = type;
				EntityId = entityId;
				Tag = tag;
				Key = BuildKey(category, type, entityId, tag);
			}

			private static string BuildKey(
				EPresentationCategory category,
				string type,
				string entityId = null,
				string tag = null)
			{
				var key = $"{category}.{type}";
				if (entityId != null) key += $".{entityId}";
				if (tag != null) key += $"#{tag}";
				return key;
			}

			public override string ToString() => Key;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Systems.Turn
{
	public class TurnQueue
	{
		private readonly List<ITurnUnit> _queue = new();

		public bool IsEmpty => _queue.Count == 0;

		public int Count => _queue.Count;

		public void Enqueue(ITurnUnit unit)
		{
			if (unit == null)
				throw new ArgumentNullException(nameof(unit));

			if (_queue.Any(u => u.Id == unit.Id))
				throw new InvalidOperationException($"[TurnQueue] Unit '{unit.Id}' is already in the queue!");

			_queue.Add(unit);
			SortQueue();
		}

		public void EnqueueRange(IEnumerable<ITurnUnit> units)
		{
			if (units == null)
				throw new ArgumentNullException(nameof(units));

			foreach (var unit in units)
			{
				if (_queue.Any(u => u.Id == unit.Id))
					throw new InvalidOperationException($"Unit '{unit.Id}' is already in the queue!");
				_queue.Add(unit);
			}
			SortQueue();
		}

		public ITurnUnit Peek() =>
			IsEmpty ? throw new InvalidOperationException("Cannot peek from empty queue!") : _queue[0];

		public ITurnUnit Dequeue()
		{
			if (IsEmpty)
				throw new InvalidOperationException("Cannot dequeue from empty queue!");

			var unit = _queue[0];
			_queue.RemoveAt(0);
			return unit;
		}

		public bool Remove(string unitId)
		{
			var unit = _queue.FirstOrDefault(u => u.Id == unitId);
			return unit != null && _queue.Remove(unit);
		}

		public bool Contains(string unitId) => _queue.Any(u => u.Id == unitId);

		public void Clear() => _queue.Clear();

		private void SortQueue()
		{
			_queue.Sort((a, b) =>
			{
				int priorityComparison = b.ActionPriority.CompareTo(a.ActionPriority);
				if (priorityComparison != 0)
					return priorityComparison;
				int speedComparison = b.Speed.CompareTo(a.Speed);
				if (speedComparison != 0)
					return speedComparison;
				return 0;
			});
		}

		public void Reorder() => SortQueue();

		public void InsertAtFront(string unitId)
		{
			var unit = _queue.FirstOrDefault(u => u.Id == unitId);
			if (unit == null)
				throw new InvalidOperationException($"Unit '{unitId}' not found in queue!");

			unit.ActionPriority = int.MaxValue;
			SortQueue();
		}

		public void InsertWithPriority(string unitId, int priority)
		{
			var unit = _queue.FirstOrDefault(u => u.Id == unitId);
			if (unit == null)
				throw new InvalidOperationException($"Unit '{unitId}' not found in queue!");

			unit.ActionPriority = priority;
			SortQueue();
		}

		public IReadOnlyList<ITurnUnit> GetQueue() => _queue.ToList();

		public int GetPositionInQueue(string unitId)
		{
			for (int i = 0; i < _queue.Count; i++)
			{
				if (_queue[i].Id == unitId)
					return i;
			}
			return -1;
		}

		public override string ToString()
		{
			if (IsEmpty)
				return "[TurnQueue] Empty";

			var units = string.Join(", ", _queue.Select(u => $"{u.Id}(S:{u.Speed},P:{u.ActionPriority})"));
			return $"[TurnQueue] Count = {Count}, Order: {units}";
		}
	}
}

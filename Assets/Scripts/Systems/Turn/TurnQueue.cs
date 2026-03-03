using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;

namespace Systems.Turn
{
	// cursor based queue for turn order
	public class TurnQueue
	{
		private readonly List<ITurnUnit> _queue = new();
		private int _currentIndex = -1;

		public int Count => _queue.Count;
		public bool IsEmpty => _queue.Count == 0;
		public bool IsExhausted => _currentIndex >= _queue.Count; // true if every unit in the queue has been advanced through

		public int CurrentIndex => _currentIndex; // raw index, for debugging only

		public ITurnUnit CurrentUnit =>
			_currentIndex >= 0 && _currentIndex < _queue.Count
				? _queue[_currentIndex]
				: null;

		public int UpcomingCount => Math.Max(0, _queue.Count - _currentIndex - 1);

		public IReadOnlyList<ITurnUnit> GetFullOrder() => _queue.AsReadOnly();

		public IReadOnlyList<ITurnUnit> GetUpcoming()
		{
			int start = _currentIndex + 1;
			return start >= _queue.Count
				? new List<ITurnUnit>()
				: _queue.GetRange(start, _queue.Count - start);
		}

		public IReadOnlyList<ITurnUnit> GetActed() =>
			_currentIndex <= 0
				? new List<ITurnUnit>()
				: _queue.GetRange(0, _currentIndex + 1);

		public int IndexOf(string unitId) => _queue.FindIndex(u => u.Id == unitId);

		public bool Contains(string unitId) => _queue.Any(u => u.Id == unitId);

		// 清除状态，重建队列
		public void Build(IEnumerable<ITurnUnit> units)
		{
			_queue.Clear();
			_currentIndex = -1;

			foreach (var unit in units)
			{
				if (unit == null)
				{
					this.LogWarning("Null unit in Build, skipping");
					continue;
				}

				if (_queue.Any(u => u.Id == unit.Id))
				{
					this.LogWarning($"Duplicate unit ID '{unit.Id}' in Build, skipping");
					continue;
				}

				_queue.Add(unit);
			}

			_queue.Sort(CompareUnits);

			this.Log($"Built with {_queue.Count} units: {FormatQueue()}");
		}

		// 前进到下一个单位，返回当前单位（如果有）
		public ITurnUnit Advance()
		{
			_currentIndex++;

			if (_currentIndex >= _queue.Count)
			{
				this.Log("Queue exhausted");
				return null;
			}

			var unit = _queue[_currentIndex];
			this.Log($"Advanced to [{_currentIndex}] '{unit.Id}' (S:{unit.Speed}, P:{unit.ActionPriority})");
			return unit;
		}

		public bool Remove(string unitId)
		{
			int index = _queue.FindIndex(u => u.Id == unitId);
			if (index == -1)
			{
				this.LogWarning($"Attempted to remove unit '{unitId}' not in queue");
				return false;
			}

			_queue.RemoveAt(index);
			if (index <= _currentIndex) _currentIndex--; // 如果移除的单位在当前或之前，调整当前索引
			this.Log($"Removed '{unitId}' at [{index}], cursor now at {_currentIndex}");
			return true;
		}

		// 添加一个单位到队列中，并重新排序可行动单位
		public void Add(ITurnUnit unit)
		{
			if (unit == null)
			{
				this.LogWarning("Cannot add null unit");
				return;
			}

			if (_queue.Any(u => u.Id == unit.Id))
			{
				this.LogWarning($"Unit '{unit.Id}' already in queue");
				return;
			}

			_queue.Add(unit);
			SortUpcoming();

			this.Log($"Added '{unit.Id}' to upcoming, queue: {FormatQueue()}");
		}

		// 设置一个单位的优先级，即刻重新排序可行动单位，但不会影响已经行动过的单位
		public bool SetPriority(string unitId, int priority)
		{
			int index = _queue.FindIndex(u => u.Id == unitId);
			if (index == -1)
			{
				this.LogWarning($"Attempted to remove unit '{unitId}' not in queue");
				return false;
			}

			if (index <= _currentIndex) // Only allow priority changes on upcoming units
			{
				this.LogWarning($"Unit '{unitId}' at [{index}] is not upcoming (cursor={_currentIndex}), cannot change priority");
				return false;
			}

			_queue[index].ActionPriority = priority;
			SortUpcoming();

			this.Log($"Set priority of '{unitId}' to {priority}, re-sorted upcoming");
			return true;
		}

		// 强制让一个单位在下一回合立刻行动，和SetPriority不同的是其会完全忽略Speed的影响
		// 实际上就是把它的ActionPriority设置成当前所有单位中最高的那个+1
		public bool MoveToNext(string unitId)
		{
			int index = _queue.FindIndex(u => u.Id == unitId);
			if (index == -1)
			{
				this.LogWarning($"Attempted to remove unit '{unitId}' not in queue");
				return false;
			}

			if (index <= _currentIndex) // Only allow priority changes on upcoming units
			{
				this.LogWarning($"Unit '{unitId}' at [{index}] is not upcoming (cursor={_currentIndex}), cannot move");
				return false;
			}

			int maxPriority = 0;
			for (int i = _currentIndex + 1; i < _queue.Count; i++)
				maxPriority = Math.Max(maxPriority, _queue[i].ActionPriority);

			_queue[index].ActionPriority = maxPriority + 1;
			SortUpcoming();

			this.Log($"Moved '{unitId}' to next in upcoming");
			return true;
		}

		// 重新排序可行动单位，通常在外部修改了单位的Speed后调用
		public void ResortUpcoming()
		{
			SortUpcoming();
			this.Log($"Resorted upcoming units, queue: {FormatQueue()}");
		}

		public void Clear()
		{
			_queue.Clear();
			_currentIndex = -1;
			this.Log("Cleared queue");
		}

		// 高 ActionPriority 优先，如果相同则高 Speed 优先
		private static int CompareUnits(ITurnUnit a, ITurnUnit b)
		{
			int p = b.ActionPriority.CompareTo(a.ActionPriority);
			return p != 0 ? p : b.Speed.CompareTo(a.Speed);
		}

		// 排序当前单位之后的所有单位（所有可行动单位）
		private void SortUpcoming()
		{
			int startIndex = _currentIndex + 1;
			if (startIndex >= _queue.Count) return;
			_queue.Sort(startIndex, _queue.Count - startIndex, Comparer<ITurnUnit>.Create(CompareUnits));
		}

		private string FormatQueue()
		{
			if (IsEmpty) return "(empty)";
			return string.Join(", ", _queue.Select((u, i) =>
			{
				string marker = i == _currentIndex ? "*" : "";
				return $"{marker}{u.Id}(S:{u.Speed},P:{u.ActionPriority})";
			}));
		}

		public override string ToString()
		{
			return $"[TurnQueue] Count={Count}, Cursor={_currentIndex}, " +
			       $"Upcoming={UpcomingCount}, Exhausted={IsExhausted}, " +
			       $"Order: {FormatQueue()}";
		}
	}
}

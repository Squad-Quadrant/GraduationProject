using System.Collections.Generic;
using Core.Log;

namespace Presentation.UI.Core
{
	public class UINavigator
	{
		private readonly List<UIPanel> _stack = new();

		public int Count => _stack.Count;
		public bool IsEmpty => _stack.Count == 0;
		public UIPanel TopPanel => _stack.Count > 0 ? _stack[^1] : null;

		public bool Push(UIPanel panel)
		{
			if (!panel)
			{
				this.LogWarning("Cannot push null panel");
				return false;
			}

			if (Contains(panel))
			{
				this.LogWarning($"Panel already in stack: {panel.PanelId}");
				return false;
			}

			_stack.Add(panel);
			this.Log($"Pushed: {panel.PanelId} | Depth: {_stack.Count}");
			return true;
		}

		public bool Remove(UIPanel panel)
		{
			if (!panel) return false;

			bool removed = _stack.Remove(panel);
			if (removed)
				this.Log($"Removed: {panel.PanelId} | Depth: {_stack.Count}");

			return removed;
		}

		public bool Contains(UIPanel panel) => panel && _stack.Contains(panel);

		public T Find<T>() where T : UIPanel
		{
			for (int i = _stack.Count - 1; i >= 0; i--)
				if (_stack[i] is T panel)
					return panel;
			return null;
		}

		public IReadOnlyList<UIPanel> GetAllPanels() => _stack;

		public void CleanupDestroyedPanels()
		{
			int removed = _stack.RemoveAll(p => !p);
			if (removed > 0)
				this.Log($"Cleaned {removed} destroyed panel(s)");
		}

		public void Clear()
		{
			// Close all panels in reverse order (top first)
			for (int i = _stack.Count - 1; i >= 0; i--)
				_stack[i]?.DoClose();
			_stack.Clear();
			this.Log("Cleared");
		}
	}
}

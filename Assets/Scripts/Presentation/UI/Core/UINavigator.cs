using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Sirenix.OdinInspector;

namespace Presentation.UI.Core
{
	[Serializable]
	public class UINavigator
	{
		private Stack<UIPanel> _panelStack = new();

		public int Count => _panelStack.Count;
		public bool IsEmpty => _panelStack.Count == 0;
		public UIPanel TopPanel => _panelStack.Count > 0 ? _panelStack.Peek() : null;

		public void Push(UIPanel panel)
		{
			if (!panel)
			{
				this.LogWarning("Trying to push a null panel onto the stack.");
				return;
			}

			if (_panelStack.Count > 0)
			{
				var top = _panelStack.Peek();
				if (top)
				{
					top.NotifyLostFocus();
					if (top.HideWhenCovered)
						top.SetVisible(false);
				}
			}

			_panelStack.Push(panel);
			panel.NotifyOpen();
			panel.NotifyFocus();

			this.Log($"Pushed: {panel.PanelName} | Stack depth: {_panelStack.Count}");
		}

		public UIPanel Pop()
		{
			CleanupDestroyedPanels();

			if (_panelStack.Count == 0)
			{
				this.LogWarning("Cannot pop from empty stack");
				return null;
			}

			var panel = _panelStack.Pop();
			panel?.NotifyLostFocus();
			panel?.NotifyClose();

			if (_panelStack.Count > 0)
			{
				var top = _panelStack.Peek();
				if (top)
				{
					if (top.HideWhenCovered)
						top.SetVisible(true);
					top.NotifyFocus();
				}
			}

			this.Log($"Popped: {panel?.PanelName ?? "null"} | Stack depth: {_panelStack.Count}");
			return panel;
		}

		public bool Remove(UIPanel panel)
		{
			if (!panel) return false;

			var tempList = new List<UIPanel>(_panelStack);
			bool wasTop = _panelStack.Count > 0 && ReferenceEquals(_panelStack.Peek(), panel);

			if (!tempList.Remove(panel))
				return false;

			_panelStack.Clear();
			for (int i = tempList.Count - 1; i >= 0; i--)
				_panelStack.Push(tempList[i]);

			// If we removed the top panel, notify new top
			if (wasTop && _panelStack.Count > 0)
			{
				var top = _panelStack.Peek();
				if (top)
				{
					if (top.HideWhenCovered)
						top.SetVisible(true);
					top.NotifyFocus();
				}
			}

			this.Log($"Removed: {panel.PanelName} | Stack depth: {_panelStack.Count}");
			return true;
		}

		public bool HandleBack()
		{
			CleanupDestroyedPanels();

			if (_panelStack.Count == 0)
				return false;

			var top = _panelStack.Peek();
			if (!top) return false;

			// Let panel handle back first (for multi-level menus)
			if (top.OnBackPressed())
			{
				this.Log($"Back consumed by: {top.PanelName}");
				return true;
			}

			// Panel didn't consume, so pop it
			Pop();
			return true;
		}

		public void CleanupDestroyedPanels()
		{
			if (_panelStack.Count == 0) return;

			var validPanels = _panelStack.Where(panel => panel).ToList();

			if (validPanels.Count == _panelStack.Count) return;

			_panelStack.Clear();
			for (int i = validPanels.Count - 1; i >= 0; i--)
				_panelStack.Push(validPanels[i]);

			this.Log($"Cleaned up {_panelStack.Count - validPanels.Count} destroyed panel(s)");
		}

		public void Clear()
		{
			_panelStack.Clear();
			this.Log("Navigator cleared");
		}

		public IReadOnlyList<UIPanel> GetAllPanels()
		{
			var list = new List<UIPanel>(_panelStack);
			list.Reverse(); // Stack enumerates top-to-bottom, we want bottom-to-top
			return list;
		}

		public bool Contains(UIPanel panel) => panel && _panelStack.Any(p => ReferenceEquals(p, panel));
	}
}

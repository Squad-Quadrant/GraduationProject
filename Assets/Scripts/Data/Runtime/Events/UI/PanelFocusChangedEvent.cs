using Core.Events;
using Presentation.UI.Core;

namespace Data.Runtime.Events.UI
{
	public readonly struct PanelFocusChangedEvent : IEvent
	{
		public string PanelId { get; }
		public UIPanel Panel { get; }
		public bool HasFocus { get; }

		public PanelFocusChangedEvent(string panelId, UIPanel panel, bool hasFocus)
		{
			PanelId = panelId;
			Panel = panel;
			HasFocus = hasFocus;
		}
	}
}

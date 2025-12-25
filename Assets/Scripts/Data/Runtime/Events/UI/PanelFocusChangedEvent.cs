using Core.Events;
using Presentation.UI.Core;

namespace Data.Runtime.Events.UI
{
	public readonly struct PanelFocusChangedEvent : IEvent
	{
		public UIPanel Panel { get; }
		public bool HasFocus { get; }

		public PanelFocusChangedEvent(UIPanel panel, bool hasFocus)
		{
			Panel = panel;
			HasFocus = hasFocus;
		}

		public override string ToString()
			=> $"[PanelFocusChanged] Id:{Panel.PanelId}, HasFocus:{HasFocus}";
	}
}

using Core.Events;
using Presentation.UI.Core;

namespace Data.Runtime.Events.UI
{
	public readonly struct PanelOpenedEvent : IEvent
	{
		public UIPanel Panel { get; }

		public PanelOpenedEvent(UIPanel panel)
		{
			Panel = panel;
		}

		public override string ToString()
			=> $"[PanelOpened] Id: {Panel.Config.PanelId}";
	}
}

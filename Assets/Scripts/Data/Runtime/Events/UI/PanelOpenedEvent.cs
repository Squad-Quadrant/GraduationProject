using Core.Events;
using Presentation.UI.Core;

namespace Data.Runtime.Events.UI
{
	public readonly struct PanelOpenedEvent : IEvent
	{
		public string PanelId { get; }
		public UIPanel Panel { get; }

		public PanelOpenedEvent(string panelId, UIPanel panel)
		{
			PanelId = panelId;
			Panel = panel;
		}

		public override string ToString()
			=> $"[PanelOpened] Id:{PanelId}";
	}
}

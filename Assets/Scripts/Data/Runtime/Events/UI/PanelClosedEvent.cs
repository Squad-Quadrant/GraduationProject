using Core.Events;
using Presentation.UI.Core;

namespace Data.Runtime.Events.UI
{
	public readonly struct PanelClosedEvent : IEvent
	{
		public string PanelId { get; }
		public UIPanel Panel { get; }

		public PanelClosedEvent(string panelId, UIPanel panel)
		{
			PanelId = panelId;
			Panel = panel;
		}

		public override string ToString()
			=> $"[PanelClosed] Id:{PanelId}";
	}

	/// <summary>
	/// Published when all panels are closed.
	/// Useful for resuming game logic or restoring input focus.
	/// </summary>
	public readonly struct AllPanelsClosedEvent : IEvent { }
}

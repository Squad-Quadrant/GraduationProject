using Core.Events;
using Presentation.UI.Core;

namespace Data.Runtime.Events.UI
{
	public readonly struct PanelClosedEvent : IEvent
	{
		public UIPanel Panel { get; }

		public PanelClosedEvent(UIPanel panel) => Panel = panel;

		public override string ToString()
			=> $"[PanelClosed] Id: {Panel.Config.PanelId}";
	}

	/// <summary>
	/// Published when all panels are closed.
	/// Useful for resuming game logic or restoring input focus.
	/// </summary>
	public readonly struct AllPanelsClosedEvent : IEvent { }
}

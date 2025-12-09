using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.Debugger
{
	[AddComponentMenu("Debugger/Event Bus Debugger")]
	public class EventBusDebugger : MonoBehaviour
	{
		#region Connection

		[TitleGroup("Connection", order: -100)]
		[ShowInInspector, ReadOnly]
		[GUIColor("@IsConnected ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.4f, 0.4f)")]
		private bool IsConnected => _eventBus != null;

		[TitleGroup("Connection")]
		[ShowInInspector, ReadOnly, DisplayAsString]
		[HideIf("IsConnected")]
		private string ConnectionHint => "Waiting for target...";

		#endregion

		#region Overview

		[TitleGroup("Overview", boldTitle: true)]
		[HorizontalGroup("Overview/Stats")]
		[BoxGroup("Overview/Stats/Counts")]
		[ShowInInspector, ReadOnly]
		[LabelText("Event Types"), LabelWidth(90)]
		[GUIColor("@EventTypeCount > 0 ? new Color(0.3f, 0.8f, 1f) : new Color(0.5f, 0.5f, 0.5f)")]
		private int EventTypeCount => _eventBus?.SubscribedEventTypeCount ?? 0;

		[BoxGroup("Overview/Stats/Counts")]
		[ShowInInspector, ReadOnly]
		[LabelText("Subscriptions"), LabelWidth(90)]
		[GUIColor("@TotalSubscriptions > 0 ? new Color(0.3f, 1f, 0.6f) : new Color(0.5f, 0.5f, 0.5f)")]
		private int TotalSubscriptions => _eventBus?.TotalSubscriptionCount ?? 0;

		[BoxGroup("Overview/Stats/Counts")]
		[ShowInInspector, ReadOnly]
		[LabelText("History Size"), LabelWidth(90)]
		[GUIColor("@HistorySize > 0 ? new Color(1f, 0.8f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
		private int HistorySize => _eventBus?.GetEventHistory().Count ?? 0;

		[BoxGroup("Overview/Stats/Settings")]
		[ShowInInspector]
		[LabelText("Record History"), LabelWidth(100)]
		[EnableIf("IsConnected")]
		private bool RecordHistory
		{
			get => _eventBus?.RecordHistory ?? false;
			set { if (_eventBus != null) _eventBus.RecordHistory = value; }
		}

		[BoxGroup("Overview/Stats/Settings")]
		[ShowInInspector]
		[LabelText("Max History"), LabelWidth(100)]
		[EnableIf("IsConnected")]
		[PropertyRange(10, 200)]
		private int MaxHistorySize
		{
			get => _eventBus?.MaxHistorySize ?? 50;
			set { if (_eventBus != null) _eventBus.MaxHistorySize = value; }
		}

		#endregion

		#region Subscriptions

		[TitleGroup("Active Subscriptions", "Currently registered event handlers")]
		[ShowInInspector, ReadOnly]
		[TableList(
			AlwaysExpanded = true,
			IsReadOnly = true,
			ShowIndexLabels = false
		)]
		[InfoBox("No active subscriptions", InfoMessageType.None, VisibleIf = "@Subscriptions.Count == 0")]
		private List<SubscriptionEntry> Subscriptions
		{
			get
			{
				if (_eventBus == null)
					return new List<SubscriptionEntry>();

				var infos = _eventBus.GetSubscriptionInfos();
				if (infos == null || infos.Count == 0)
					return new List<SubscriptionEntry>();

				return infos
					.OrderByDescending(i => i.SubscriberCount)
					.Select(i => new SubscriptionEntry
					{
						eventType = i.EventType.Name,
						subscribers = i.SubscriberCount,
						onceOnly = i.OnceCount
					})
					.ToList();
			}
		}

		#endregion

		#region Event History

		[TitleGroup("Event History", "Recently published events (newest first)")]
		[ShowInInspector, ReadOnly]
		[TableList(
			AlwaysExpanded = true,
			IsReadOnly = true,
			ShowIndexLabels = false,
			NumberOfItemsPerPage = 15
		)]
		[InfoBox("No events recorded", InfoMessageType.None, VisibleIf = "@EventHistory.Count == 0")]
		private List<EventHistoryDisplayEntry> EventHistory
		{
			get
			{
				if (_eventBus == null)
					return new List<EventHistoryDisplayEntry>();

				var history = _eventBus.GetEventHistory();
				if (history == null || history.Count == 0)
					return new List<EventHistoryDisplayEntry>();

				return history.Select(h => new EventHistoryDisplayEntry
				{
					time = h.Timestamp.ToString("HH:mm:ss.fff"),
					eventType = h.EventTypeName,
					data = TruncateString(h.EventData, 60),
					handlers = h.HandlerCount
				}).ToList();
			}
		}

		#endregion

		#region Event Details

		[TitleGroup("Event Details", "Expand event data")]
		[ShowInInspector]
		[LabelText("Selected Index"), LabelWidth(100)]
		[PropertyRange(0, "@Mathf.Max(0, HistorySize - 1)")]
		[EnableIf("@HistorySize > 0")]
		private int _selectedEventIndex;

		[TitleGroup("Event Details")]
		[ShowInInspector, ReadOnly]
		[LabelText("Full Data")]
		[ShowIf("@HistorySize > 0")]
		private string SelectedEventFullData
		{
			get
			{
				if (_eventBus == null) return "";
				var history = _eventBus.GetEventHistory();
				if (history == null || _selectedEventIndex >= history.Count) return "";
				return history[_selectedEventIndex].EventData;
			}
		}

		#endregion

		#region Control Panel

		[TitleGroup("Control Panel")]
		[HorizontalGroup("Control Panel/Row1")]
		[Button("Clear History", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.3f)]
		[EnableIf("@IsConnected && HistorySize > 0")]
		private void ClearHistory()
		{
			_eventBus?.ClearHistory();
			Debug.Log("[EventBusDebugger] History cleared");
		}

		[HorizontalGroup("Control Panel/Row1")]
		[Button("Clear All Subscriptions", ButtonSizes.Large), GUIColor(1f, 0.3f, 0.3f)]
		[EnableIf("@IsConnected && EventTypeCount > 0")]
		private void ClearAllSubscriptions()
		{
			_eventBus?.Clear();
			Debug.Log("[EventBusDebugger] All subscriptions cleared");
		}

		#endregion

		#region Filter

		[TitleGroup("Filter", "Filter displayed events")]
		[HorizontalGroup("Filter/Row1")]
		[SerializeField]
		[LabelText("Event Type Filter"), LabelWidth(110)]
		private string eventTypeFilter = "";

		[HorizontalGroup("Filter/Row1", Width = 80)]
		[Button("Apply"), GUIColor(0.3f, 0.8f, 1f)]
		private void ApplyFilter()
		{
			// Filter is applied automatically via property
			Debug.Log($"[EventBusDebugger] Filter applied: '{eventTypeFilter}'");
		}

		[HorizontalGroup("Filter/Row1", Width = 80)]
		[Button("Clear"), GUIColor(0.8f, 0.8f, 0.8f)]
		private void ClearFilter()
		{
			eventTypeFilter = "";
			Debug.Log("[EventBusDebugger] Filter cleared");
		}

		[TitleGroup("Filtered History")]
		[ShowInInspector, ReadOnly]
		[TableList(
			AlwaysExpanded = true,
			IsReadOnly = true,
			ShowIndexLabels = false,
			NumberOfItemsPerPage = 10
		)]
		[ShowIf("@!string.IsNullOrEmpty(eventTypeFilter)")]
		private List<EventHistoryDisplayEntry> FilteredEventHistory
		{
			get
			{
				if (_eventBus == null || string.IsNullOrEmpty(eventTypeFilter))
					return new List<EventHistoryDisplayEntry>();

				var history = _eventBus.GetEventHistory();
				if (history == null || history.Count == 0)
					return new List<EventHistoryDisplayEntry>();

				var filter = eventTypeFilter.ToLowerInvariant();

				return history
					.Where(h => h.EventTypeName.ToLowerInvariant().Contains(filter))
					.Select(h => new EventHistoryDisplayEntry
					{
						time = h.Timestamp.ToString("HH:mm:ss.fff"),
						eventType = h.EventTypeName,
						data = TruncateString(h.EventData, 60),
						handlers = h.HandlerCount
					})
					.ToList();
			}
		}

		#endregion

		#region Private Fields

		private EventBus _eventBus;

		#endregion

		#region Unity Lifecycle

		private void OnEnable()
		{
			if (Application.isPlaying)
				TryConnect();
		}

		private void Update()
		{
			if (Application.isPlaying && _eventBus == null)
				TryConnect();
		}

		private void OnDisable() => _eventBus = null;

		#endregion

		#region Connection

		private void TryConnect()
		{
			if (!RootContainer.Instance)
				return;

			// EventBus is registered in RootContainer
			var bus = RootContainer.Instance.TryResolve<IEventBus>();
			_eventBus = bus as EventBus;
		}

		#endregion

		#region Helper Methods

		private static string TruncateString(string str, int maxLength)
		{
			if (string.IsNullOrEmpty(str)) return "";
			if (str.Length <= maxLength) return str;
			return str.Substring(0, maxLength - 3) + "...";
		}

		#endregion

		#region Display Data Structures

		/// <summary>
		/// Display structure for subscription information.
		/// </summary>
		[Serializable]
		private struct SubscriptionEntry
		{
			[TableColumnWidth(180)]
			[LabelText("Event Type")]
			public string eventType;

			[TableColumnWidth(80, Resizable = false)]
			[GUIColor("@subscribers > 5 ? new Color(1f, 0.8f, 0.3f) : new Color(1f, 1f, 1f)")]
			public int subscribers;

			[TableColumnWidth(70, Resizable = false)]
			[LabelText("Once")]
			[GUIColor("@onceOnly > 0 ? new Color(0.8f, 0.5f, 1f) : new Color(0.7f, 0.7f, 0.7f)")]
			public int onceOnly;
		}

		/// <summary>
		/// Display structure for event history entries.
		/// </summary>
		[Serializable]
		private struct EventHistoryDisplayEntry
		{
			[TableColumnWidth(90, Resizable = false)]
			[GUIColor(0.7f, 0.7f, 0.7f)]
			public string time;

			[TableColumnWidth(160)]
			[LabelText("Event")]
			[GUIColor(0.3f, 0.8f, 1f)]
			public string eventType;

			[TableColumnWidth(250)]
			public string data;

			[TableColumnWidth(60, Resizable = false)]
			[LabelText("#")]
			[GUIColor("@handlers == 0 ? new Color(1f, 0.4f, 0.4f) : new Color(0.3f, 1f, 0.6f)")]
			public int handlers;
		}

		#endregion
	}
}

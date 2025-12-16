using Core.Log;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.UI.Core
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class UIPanel : MonoBehaviour
	{
		[TitleGroup("Panel Settings")]
		[SerializeField]
		[LabelText("Panel Name")]
		[Tooltip("Identifier for debugging and logging. Uses GameObject name if empty.")]
		private string panelName;

		[TitleGroup("Panel Settings")]
		[SerializeField]
		[LabelText("Hide When Covered")]
		[Tooltip("If true, this panel becomes invisible when another panel is pushed on top.")]
		private bool hideWhenCovered;

		[TitleGroup("Panel Settings")]
		[SerializeField]
		[LabelText("Block Input When Open")]
		[Tooltip("If true, blocks raycasts to UI elements behind this panel.")]
		private bool blockInputWhenOpen = true;

		[TitleGroup("Runtime State")]
		[ShowInInspector, ReadOnly]
		[GUIColor("@IsOpen ? new Color(0.3f, 1f, 0.3f) : new Color(0.5f, 0.5f, 0.5f)")]
		public bool IsOpen { get; private set; }

		[TitleGroup("Runtime State")]
		[ShowInInspector, ReadOnly]
		[GUIColor("@HasFocus ? new Color(0.3f, 0.8f, 1f) : new Color(0.5f, 0.5f, 0.5f)")]
		public bool HasFocus { get; private set; }

		[TitleGroup("Runtime State")]
		[ShowInInspector, ReadOnly]
		public bool IsVisible { get; private set; } = true;

		public string PanelName => string.IsNullOrEmpty(panelName) ? gameObject.name : panelName;

		public bool HideWhenCovered => hideWhenCovered;

		public CanvasGroup CanvasGroup
		{
			get
			{
				if (!_canvasGroup)
					_canvasGroup = GetComponent<CanvasGroup>();
				return _canvasGroup;
			}
		}
		private CanvasGroup _canvasGroup;

		private bool _isDestroying;

		protected virtual void OnDestroy()
		{
			if (_isDestroying) return;
			_isDestroying = true;

			var manager = RootContainer.Instance.Resolve<UIManager>();
			if (manager && manager.Navigator != null)
				manager.Navigator.Remove(this);
		}

		public void SetVisible(bool visible)
		{
			IsVisible = visible;
			CanvasGroup.alpha = visible ? 1 : 0;
			CanvasGroup.interactable = visible;
			CanvasGroup.blocksRaycasts = visible && blockInputWhenOpen;
		}

		internal void NotifyOpen()
		{
			if (IsOpen) return;

			IsOpen = true;
			SetVisible(true);

			this.Log($"Opened: {PanelName}");
			OnOpen();
		}

		internal void NotifyClose()
		{
			if (!IsOpen) return;

			IsOpen = false;
			HasFocus = false;

			this.Log($"Closed: {PanelName}");
			OnClose();
		}

		internal void NotifyFocus()
		{
			if (HasFocus) return;

			HasFocus = true;
			CanvasGroup.interactable = true;
			CanvasGroup.blocksRaycasts = blockInputWhenOpen;

			this.Log($"Focused: {PanelName}");
			OnFocus();
		}

		internal void NotifyLostFocus()
		{
			if (!HasFocus) return;

			HasFocus = false;
			CanvasGroup.interactable = false;

			this.Log($"Lost focus: {PanelName}");
			OnLostFocus();
		}

		protected virtual void OnOpen() { }

		protected virtual void OnClose() { }

		protected virtual void OnFocus() { }

		protected virtual void OnLostFocus() { }

		/// <summary>
		/// Handle back/ESC input
		/// </summary>
		/// <returns>True if input was consumed, false to allow pop.</returns>
		public virtual bool OnBackPressed() => false;

		#region Debug Action

		[Button("Open Panel"), GUIColor(0.3f, 1f, 0.5f)]
		[HorizontalGroup("Debug Actions")]
		[EnableIf("@UnityEngine.Application.isPlaying && !IsOpen")]
		public void Open()
		{
			if (IsOpen)
			{
				this.LogWarning($"Panel '{PanelName}' is already open");
				return;
			}

			var manager = RootContainer.Instance.Resolve<UIManager>();
			if (!manager)
			{
				this.LogError($"Cannot open '{PanelName}': UIManager not found");
				return;
			}
			manager.Navigator.Push(this);
		}

		[Button("Close Panel"), GUIColor(1f, 0.5f, 0.3f)]
		[HorizontalGroup("Debug Actions")]
		[EnableIf("@UnityEngine.Application.isPlaying && IsOpen")]
		public void Close()
		{
			if (!IsOpen)
			{
				this.LogWarning($"Panel '{PanelName}' is not open");
				return;
			}

			var manager = RootContainer.Instance.Resolve<UIManager>();
			if (!manager)
			{
				this.LogError($"Cannot open '{PanelName}': UIManager not found");
				return;
			}

			// Use Pop if we're the top panel, Remove otherwise
			if (manager.Navigator.TopPanel == this)
				manager.Navigator.Pop();
			else
				manager.Navigator.Remove(this);
		}

		#endregion
	}
}

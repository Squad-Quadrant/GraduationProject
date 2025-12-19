using System;
using Core.Log;
using Data.Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Presentation.UI.Core
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class UIPanel : MonoBehaviour
	{
		#region Runtime State

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
		public bool IsVisible { get; private set; }

		[TitleGroup("Runtime State")]
		[ShowInInspector, ReadOnly]
		public bool IsAnimating => _animation?.IsAnimating ?? false;

		public string PanelId => _config?.PanelId ?? gameObject.name;

		#endregion

		private UIPanelConfig _config;
		private IUIAnimation _animation;

		public bool ManagedByStack => _config?.ManagedByStack ?? true;
		public bool HideWhenCovered => _config?.HideWhenCovered ?? false;
		public bool CloseOnBack => _config?.CloseOnBack ?? true;
		public bool BlockInput => _config?.BlockInput ?? true;
		public bool CacheOnClose => _config?.CacheOnClose ?? false;

		private CanvasGroup _canvasGroup;
		public CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();

		internal void Initialize(UIPanelConfig config)
		{
			_config = config;
			_animation = GetComponent<IUIAnimation>();

			SetVisibleImmediate(false);
			IsOpen = false;
			HasFocus = false;

			OnInitialize();
			this.Log($"Initialized: {PanelId}");
		}

		protected virtual void OnDestroy()
		{
			_animation?.CompleteImmediately();
			this.Log($"Destroyed: {PanelId}");
		}

		public virtual bool OnBackPressed() => false;

		protected virtual void OnInitialize() { }
		protected virtual void OnOpen() { }
		protected virtual void OnClose() { }
		protected virtual void OnFocus() { }
		protected virtual void OnUnfocus() { }

		internal void DoOpen(Action onComplete = null)
		{
			if (IsOpen)
			{
				onComplete?.Invoke();
				return;
			}

			IsOpen = true;
			gameObject.SetActive(true);

			if (_animation != null)
			{
				SetVisibleImmediate(true);
				CanvasGroup.interactable = false;
				_animation.PlayOpen(() =>
				{
					this.Log($"Opened: {PanelId}");
					OnOpen();
					onComplete?.Invoke();
				});
			}
			else
			{
				SetVisibleImmediate(true);
				this.Log($"Opened: {PanelId}");
				OnOpen();
				onComplete?.Invoke();
			}
		}

		internal void DoClose(Action onComplete = null)
		{
			if (!IsOpen)
			{
				onComplete?.Invoke();
				return;
			}

			HasFocus = false;
			CanvasGroup.interactable = false;

			if (_animation != null)
				_animation.PlayClose(CompleteClose);
			else
				CompleteClose();
			return;

			void CompleteClose()
			{
				IsOpen = false;
				SetVisibleImmediate(false);
				this.Log($"Closed: {PanelId}");
				OnClose();
				onComplete?.Invoke();
			}
		}

		internal void DoCloseImmediate()
		{
			_animation?.CompleteImmediately();
			IsOpen = false;
			HasFocus = false;
			SetVisibleImmediate(false);
			OnClose();
		}

		internal void DoFocus()
		{
			if (HasFocus) return;

			HasFocus = true;
			CanvasGroup.interactable = true;
			CanvasGroup.blocksRaycasts = BlockInput;

			this.Log($"Focused: {PanelId}");
			OnFocus();
		}

		internal void DoUnfocus()
		{
			if (!HasFocus) return;

			HasFocus = false;
			CanvasGroup.interactable = false;

			this.Log($"Unfocused: {PanelId}");
			OnUnfocus();
		}

		internal void DoShow(Action onComplete = null)
		{
			if (IsVisible)
			{
				onComplete?.Invoke();
				return;
			}

			SetVisibleImmediate(true);

			if (_animation != null)
				_animation.PlayShow(onComplete);
			else
				onComplete?.Invoke();
		}

		internal void DoHide(Action onComplete = null)
		{
			if (!IsVisible)
			{
				onComplete?.Invoke();
				return;
			}

			if (_animation != null)
			{
				_animation.PlayHide(() =>
				{
					SetVisibleImmediate(false);
					onComplete?.Invoke();
				});
			}
			else
			{
				SetVisibleImmediate(false);
				onComplete?.Invoke();
			}
		}

		private void SetVisibleImmediate(bool visible)
		{
			IsVisible = visible;
			CanvasGroup.alpha = visible ? 1f : 0f;
			CanvasGroup.interactable = visible && HasFocus;
			CanvasGroup.blocksRaycasts = visible && BlockInput;
		}

		public void SetVisible(bool visible, bool animate = true)
		{
			if (visible)
			{
				if (animate)
					DoShow();
				else
					SetVisibleImmediate(true);
			}
			else
			{
				if (animate)
					DoHide();
				else
					SetVisibleImmediate(false);
			}
		}
	}
}

using System;
using Core.Events;
using Core.Log;
using Data.Config;
using Presentation.Bootstrap;
using Presentation.UI.Config;
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
		[GUIColor("@IsAnimating ? new Color(0.3f, 0.8f, 1f) : new Color(0.5f, 0.5f, 0.5f)")]
		public bool IsAnimating => _animation?.IsAnimating ?? false;

		public UIPanelConfig Config { get; private set; }

		#endregion

		private IUIAnimation _animation;
		private CanvasGroup _canvasGroup;
		public CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();

		private IEventBus _eventBus;
		protected IEventBus EventBus => _eventBus ??= RootContainer.Instance.Resolve<IEventBus>();

		internal void Initialize(UIPanelConfig config)
		{
			if (!config)
			{
				this.LogError("Cannot initialize panel: config is null");
				return;
			}

			Config = config;
			_animation = GetComponent<IUIAnimation>();

			SetVisible(false);
			IsOpen = false;

			OnInitialize();
			this.Log($"Initialized: {Config.PanelId}");
		}

		internal void Open(Action onComplete = null)
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
				SetVisible(true);
				CanvasGroup.interactable = false;
				_animation.PlayOpen(() =>
				{
					CanvasGroup.interactable = true;
					OnOpen();
					onComplete?.Invoke();
					this.Log($"Opened (animated): {Config.PanelId}");
				});
			}
			else
			{
				SetVisible(true);
				OnOpen();
				onComplete?.Invoke();
				this.Log($"Opened: {Config.PanelId}");
			}
		}

		internal void Close(Action onComplete = null)
		{
			if (!IsOpen)
			{
				onComplete?.Invoke();
				return;
			}

			CanvasGroup.interactable = false;

			if (_animation != null)
			{
				_animation.PlayClose(() =>
				{
					IsOpen = false;
					SetVisible(false);
					OnClose();
					onComplete?.Invoke();
					this.Log($"Closed (animated): {Config.PanelId}");
				});
			}
			else
			{
				IsOpen = false;
				SetVisible(false);
				OnClose();
				onComplete?.Invoke();
				this.Log($"Closed: {Config.PanelId}");
			}
		}

		internal void CloseImmediate()
		{
			if (!IsOpen) return;

			_animation?.CompleteImmediately();
			IsOpen = false;
			SetVisible(false);
			OnClose();
			this.Log($"Closed immediately: {Config.PanelId}");
		}

		protected virtual void OnDestroy()
		{
			_animation?.CompleteImmediately();
			this.Log($"Destroyed: {Config.PanelId}");
		}

		protected virtual void OnInitialize() { }
		protected virtual void OnOpen() { }
		protected virtual void OnClose() { }

		private void SetVisible(bool visible)
		{
			CanvasGroup.alpha = visible ? 1f : 0f;
			CanvasGroup.interactable = visible;
			CanvasGroup.blocksRaycasts = visible;
		}
	}
}

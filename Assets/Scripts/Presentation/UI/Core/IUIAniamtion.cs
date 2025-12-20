using System;

namespace Presentation.UI.Core
{
	public interface IUIAnimation
	{
		void PlayOpen(Action onComplete);

		void PlayClose(Action onComplete);

		void PlayHide(Action onComplete);

		void PlayShow(Action onComplete);

		void CompleteImmediately();

		bool IsAnimating { get; }
	}
}

using System;

namespace Presentation.UI.Core
{
	public interface IUIAnimation
	{
		void PlayOpen(Action onComplete);

		void PlayClose(Action onComplete);

		void CompleteImmediately();

		bool IsAnimating { get; }
	}
}

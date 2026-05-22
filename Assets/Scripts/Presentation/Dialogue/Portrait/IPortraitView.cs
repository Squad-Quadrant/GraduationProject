using System;
using Presentation.Dialogue.Config;

namespace Presentation.Dialogue.Portrait
{
	public interface IPortraitView
	{
		void Setup(CharacterConfig character, string poseName, string skinName);

		void PlayEntrance(EPortraitPosition position, string entryAnimation, Action onComplete);

		void PlayExit(EPortraitPosition position, Action onComplete);

		void ChangeAppearance(string poseName, string skinName, string entryAnimation, Action onComplete);
	}
}

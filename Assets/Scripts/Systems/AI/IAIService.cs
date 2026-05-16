using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.AI
{
	// ReSharper disable once InconsistentNaming
	public interface IAIService
	{
		void ExecuteTurn(Unit.Unit unit, Action onComplete);
	}
}

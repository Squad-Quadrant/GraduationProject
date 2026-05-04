using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.AI
{
	// ReSharper disable once InconsistentNaming
	public interface IAIService
	{
		void ExecuteTurn(Unit.Unit unit, Action onComplete);
        
        void AddObscuresCells(List<Vector2Int> cells);
        
        void RemoveObscuresCells(List<Vector2Int> cells);
        
        void RemoveAllObscuresCells(List<Vector2Int> cells);
	}
}

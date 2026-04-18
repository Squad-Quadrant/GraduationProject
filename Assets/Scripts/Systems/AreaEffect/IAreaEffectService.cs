using System.Collections.Generic;
using UnityEngine;

namespace Systems.AreaEffect
{
	public interface IAreaEffectService
	{
		IReadOnlyCollection<AreaEffect> GetAll();

		IReadOnlyList<AreaEffect> GetAt(Vector2Int cell);

		bool TryGet(string effectId, out AreaEffect effect);

		AreaEffect Register(
			string ownerId,
			Vector2Int targetCell,
			IReadOnlyList<Vector2Int> cells,
			int remainingTurns,
			AreaEffectBehavior behavior);

		void Unregister(string effectId);
	}
}

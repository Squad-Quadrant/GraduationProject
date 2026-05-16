using System.Collections.Generic;

namespace Systems.Turn.Activation
{
	public interface IEnemyActivationService
	{
		bool IsActivated(string unitId);

		void Activate(string unitId);

		IReadOnlyCollection<string> GetActivatedUnits();
	}
}

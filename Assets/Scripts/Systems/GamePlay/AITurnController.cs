using Core.Log;
using Systems.AI;
using Systems.Turn;

namespace Systems.GamePlay
{
	public class AITurnController : ITurnController
	{
		private readonly ITurnService _turnService;
		private readonly IAIService _aiService;

		public AITurnController(ITurnService turnService, IAIService aiService)
		{
			_turnService = turnService;
			_aiService = aiService;
		}

		public void BeginTurn(ITurnUnit turnUnit)
		{
			if (turnUnit is not Unit.Unit unit)
			{
				this.LogError("TurnUnit is not 'Unit.Unit', skipping turn");
				_turnService.EndUnitTurn();
				return;
			}

			this.Log($"AI control: '{unit.name}'");
			_aiService.ExecuteTurn(unit, () =>
			{
				this.Log($"AI finished: '{unit.name}' — ending unit turn");
				_turnService.EndUnitTurn();
			});
		}
	}
}

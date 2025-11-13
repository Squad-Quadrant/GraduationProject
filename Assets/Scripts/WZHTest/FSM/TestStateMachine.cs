using Core.Events;
using Core.FSM;
using Core.Log;
using Presentation.Bootstrap;
using Sirenix.OdinInspector;
using UnityEngine;
using WZHTest.FSM.States;

namespace WZHTest.FSM
{
	public class TestStateMachine : MonoBehaviour
	{
		[SerializeField] [ReadOnly] private TurnContext context;
		[SerializeField] private TestDebugger debugger;
		private StateMachine<TurnContext> _fsm;
		private PlayerTurnState _playerTurnState;
		private EnemyTurnState _enemyTurnState;

		[Button]
		private void Setup()
		{
			var logger = RootContainer.Instance.Resolve<ILog>();
			var eventBus = RootContainer.Instance.Resolve<IEventBus>();

			context = new TurnContext();
			_fsm = new StateMachine<TurnContext>(context, logger, eventBus, name: "TestStateMachine");

			_playerTurnState = new PlayerTurnState();
			_enemyTurnState = new EnemyTurnState();

			_fsm.AddTransition(new FuncTransition<TurnContext>(
				from: _playerTurnState,
				to: _enemyTurnState,
				condition: ctx => ctx.turnTimer >= ctx.maxTurnTime
				));

			_fsm.AddTransition(new FuncTransition<TurnContext>(
				from: _enemyTurnState,
				to: _playerTurnState,
				condition: ctx => ctx.turnTimer >= ctx.maxTurnTime
				));

			eventBus.Subscribe<StateChangedEvent<TurnContext>>(OnStateChange);

			_fsm.ChangeState(_playerTurnState);

			debugger.SetStateMachine(_fsm);
		}

		private void OnStateChange(StateChangedEvent<TurnContext> obj)
		{
			Debug.Log($"状态变更: {obj.PreviousState?.Name ?? "null"} -> {obj.CurrentState.Name}");
		}

		private void Update() => _fsm?.Update(Time.deltaTime);
	}
}

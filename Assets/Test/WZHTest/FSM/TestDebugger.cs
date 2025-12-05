using Core.FSM;
using Presentation.Debugger;

namespace Test.WZHTest.FSM
{
	public class TestDebugger : StateMachineDebuggerBase<TurnContext>
	{
		protected override StateMachine<TurnContext> FindStateMachine()
		{
			throw new System.NotImplementedException();
		}
	}
}

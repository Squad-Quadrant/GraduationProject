namespace PurpleFlowerCore.FSM
{
	public interface IStateMachine 
	{
		IState CurrentState{ get; }
		
		IState DefaultState{ get; set; }
		
		void AddState (IState state);
		
		void RemoveState(IState state);
		
		IState GetStateWithTag(string tag);
		
        void AddAnyState(ITransition t);
	}
}

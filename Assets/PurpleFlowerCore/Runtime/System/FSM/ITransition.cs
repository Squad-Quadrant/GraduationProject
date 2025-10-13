namespace PurpleFlowerCore.FSM
{
	public interface ITransition 
	{
		IState From{get;set;}
		
		IState To{get;set;}
		
		string Name{ get; set; }
		
		bool TransitionCallback();
		
		bool ShouldBegin ();
	}
}

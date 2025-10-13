namespace PurpleFlowerCore.FSM
{
	public delegate bool LTransitionDelegate();
	
	public class LTransition : ITransition 
	{
		private IState _from; // 原状态
		private IState _to;	// 目标状态
		private string _name; // 过渡名
		
		public event LTransitionDelegate OnTransition;
		public event LTransitionDelegate OnCheck;
		
		public IState From 
		{
			get => _from;
			set => _from = value;
		}
		
		public IState To 
		{
			get => _to;
			set => _to = value;
		}

		public string Name 
		{
			get => _name;
			set => _name = value;
		}
		
		public LTransition(string name,IState fromState,IState toState)
		{
			_name = name;
			_from = fromState;
			_to = toState;
		}
		
		public bool TransitionCallback()
		{
			return OnTransition == null || OnTransition();
		}

		public bool ShouldBegin ()
		{
			return OnCheck!=null && OnCheck ();
		}
	}
}

using Core.Log;

namespace Systems.Interaction.States
{
	public class WaitingForSystemState : InteractionState
	{
		public WaitingForSystemState() : base(InteractionStates.WaitingForSystem) { }

		public override void OnEnter(InteractionContext ctx)
		{
			base.OnEnter(ctx);
			this.Log("Entered — waiting for GameServer to advance");
		}

		public override void OnExit(InteractionContext ctx)
		{
			this.Log("Exited — GameServer has resumed control");
			base.OnExit(ctx);
		}
	}
}

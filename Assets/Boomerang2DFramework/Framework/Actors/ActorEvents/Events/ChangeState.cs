namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class ChangeState : ActorEvent {
		private ChangeStateProperties MyProperties => (ChangeStateProperties) Properties;

		public ChangeState(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			targetActor.StateMachine.SetNextState(MyProperties.State);
		}
	}
}
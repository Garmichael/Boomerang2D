using Boomerang2DFramework.Framework.AudioManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class ResetTimeInState : ActorEvent {
		public ResetTimeInState(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			targetActor.StateMachine.GetCurrentState().TimeInState = 0;
		}
	}
}
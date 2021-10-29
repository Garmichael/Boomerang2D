namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetVelocity : ActorEvent {
		private SetVelocityProperties MyProperties => (SetVelocityProperties) Properties;

		public SetVelocity(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			if (MyProperties.UpdateHorizontalVelocity) {
				targetActor.SetVelocityX(MyProperties.HorizontalVelocity.BuildValue(targetActor));
			}

			if (MyProperties.UpdateVerticalVelocity) {
				targetActor.SetVelocityY(MyProperties.VerticalVelocity.BuildValue(targetActor));
			}
		}
	}
}
namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class ChangeFacingDirection : ActorEvent {
		private ChangeFacingDirectionProperties MyProperties => (ChangeFacingDirectionProperties) Properties;

		public ChangeFacingDirection(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			targetActor.FacingDirection = MyProperties.NewFacingDirection;
		}
	}
}
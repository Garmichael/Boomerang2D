namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetCameraTarget : ActorEvent {
		private SetCameraTargetProperties MyProperties => (SetCameraTargetProperties) Properties;

		public SetCameraTarget(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			Boomerang2D.MainCameraController.Target = targetActor;

			if (MyProperties.SnapCameraPosition) {
				Boomerang2D.MainCameraController.RealPosition = targetActor.RealPosition;
			}
		}
	}
}
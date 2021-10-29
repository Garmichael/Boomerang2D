namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class KillActor : ActorEvent {
		private KillActorProperties MyProperties => (KillActorProperties) Properties;

		public KillActor(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			targetActor?.Kill();
		}
	}
}
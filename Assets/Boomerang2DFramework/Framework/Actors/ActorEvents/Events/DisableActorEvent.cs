namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class DisableActorEvent : ActorEvent {
		private DisableActorEventProperties MyProperties => (DisableActorEventProperties) Properties;

		public DisableActorEvent(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			int eventId = MyProperties.DisableEventId - 1;
			if (eventId >= 0 && eventId < targetActor.ActorProperties.InteractionEvents.Count) {
				targetActor.ActorProperties.InteractionEvents[eventId].Enabled = false;
			}
		}
	}
}
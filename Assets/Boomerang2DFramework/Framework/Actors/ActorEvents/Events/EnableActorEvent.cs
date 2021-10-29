namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class EnableActorEvent : ActorEvent {
		private EnableActorEventProperties MyProperties => (EnableActorEventProperties) Properties;

		public EnableActorEvent(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			int eventId = MyProperties.EnableEventId - 1;
			if (eventId >= 0 && eventId < targetActor.ActorProperties.InteractionEvents.Count) {
				targetActor.ActorProperties.InteractionEvents[eventId].Enabled = true;
			}
		}
	}
}
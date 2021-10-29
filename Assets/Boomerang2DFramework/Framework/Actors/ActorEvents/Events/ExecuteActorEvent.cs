using Boomerang2DFramework.Framework.Actors.InteractionEvents;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class ExecuteActorEvent : ActorEvent {
		private ExecuteActorEventProperties MyProperties => (ExecuteActorEventProperties) Properties;

		public ExecuteActorEvent(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			int eventId = MyProperties.ExecuteEventId - 1;
			if (eventId >= 0 && eventId <= targetActor.ActorProperties.InteractionEvents.Count) {
				ActorInteractionEvent actorInteractionEvent = targetActor.ActorProperties.InteractionEvents[eventId];
				targetActor.ExecuteInteractionEvent(actorInteractionEvent, false);
			}
		}
	}
}
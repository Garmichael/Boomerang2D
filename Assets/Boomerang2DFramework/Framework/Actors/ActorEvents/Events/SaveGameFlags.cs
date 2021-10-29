using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SaveGameFlags : ActorEvent {
		public SaveGameFlags(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			GameFlags.SaveGameFlags();
		}
	}
}
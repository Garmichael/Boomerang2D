using Boomerang2DFramework.Framework.UiManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class CloseHudObject : ActorEvent {
		private CloseHudObjectProperties MyProperties => (CloseHudObjectProperties) Properties;

		public CloseHudObject(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			UiManager.RemoveHudObject(MyProperties.HudObject);
		}
	}
}
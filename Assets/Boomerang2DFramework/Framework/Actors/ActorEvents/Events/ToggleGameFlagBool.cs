using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class ToggleGameFlagBool : ActorEvent {
		private ToggleGameFlagBoolProperties MyFloatProperties => (ToggleGameFlagBoolProperties) Properties;

		public ToggleGameFlagBool(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			GameFlags.SetBoolFlag(MyFloatProperties.GameFlagName, GameFlags.GetBoolFlag(MyFloatProperties.GameFlagName));
		}
	}
}
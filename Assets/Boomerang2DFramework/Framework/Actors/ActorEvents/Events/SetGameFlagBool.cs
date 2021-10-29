using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetGameFlagBool : ActorEvent {
		private SetGameFlagBoolProperties MyFloatProperties => (SetGameFlagBoolProperties) Properties;

		public SetGameFlagBool(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			GameFlags.SetBoolFlag(MyFloatProperties.GameFlagName, MyFloatProperties.GameFlagValue);
		}
	}
}
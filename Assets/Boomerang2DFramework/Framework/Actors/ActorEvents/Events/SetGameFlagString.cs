using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetGameFlagString : ActorEvent {
		private SetGameFlagStringProperties MyFloatProperties => (SetGameFlagStringProperties) Properties;

		public SetGameFlagString(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			GameFlags.SetStringFlag(MyFloatProperties.GameFlagName, MyFloatProperties.GameFlagValue);
		}
	}
}
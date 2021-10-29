using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetGameFlagFloat : ActorEvent {
		private SetGameFlagFloatProperties MyFloatProperties => (SetGameFlagFloatProperties) Properties;

		public SetGameFlagFloat(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}

			GameFlags.SetFloatFlag(
				MyFloatProperties.GameFlagName,
				MyFloatProperties.GameFlagValue.BuildValue(targetActor)
			);
		}
	}
}
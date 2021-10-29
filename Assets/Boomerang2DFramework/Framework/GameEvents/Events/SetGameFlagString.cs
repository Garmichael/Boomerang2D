using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class SetGameFlagString : GameEvent {
		private SetGameFlagStringProperties MyFloatProperties => (SetGameFlagStringProperties) Properties;

		public SetGameFlagString(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			GameFlags.SetStringFlag(MyFloatProperties.GameFlagName, MyFloatProperties.GameFlagValue);
		}
	}
}
using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class SetGameFlagBool : GameEvent {
		private SetGameFlagBoolProperties MyFloatProperties => (SetGameFlagBoolProperties) Properties;

		public SetGameFlagBool(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			GameFlags.SetBoolFlag(MyFloatProperties.GameFlagName, MyFloatProperties.GameFlagValue);
		}
	}
}
using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class SetGameFlagFloat : GameEvent {
		private SetGameFlagFloatProperties MyFloatProperties => (SetGameFlagFloatProperties) Properties;

		public SetGameFlagFloat(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			GameFlags.SetFloatFlag(MyFloatProperties.GameFlagName, MyFloatProperties.GameFlagValue);
		}
	}
}
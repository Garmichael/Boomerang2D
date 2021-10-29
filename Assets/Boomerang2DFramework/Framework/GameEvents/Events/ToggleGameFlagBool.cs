using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class ToggleGameFlagBool : GameEvent {
		private ToggleGameFlagBoolProperties MyFloatProperties => (ToggleGameFlagBoolProperties) Properties;

		public ToggleGameFlagBool(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			GameFlags.SetBoolFlag(MyFloatProperties.GameFlagName, GameFlags.GetBoolFlag(MyFloatProperties.GameFlagName));
		}
	}
}
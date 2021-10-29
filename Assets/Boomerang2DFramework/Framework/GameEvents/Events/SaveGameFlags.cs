using Boomerang2DFramework.Framework.GameFlagManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class SaveGameFlags : GameEvent {
		private SaveGameFlagsProperties MyFloatProperties => (SaveGameFlagsProperties) Properties;

		public SaveGameFlags(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			GameFlags.SaveGameFlags();
		}
	}
}
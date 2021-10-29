using Boomerang2DFramework.Framework.UiManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class CloseHudObject : GameEvent {
		private CloseHudObjectProperties MyProperties => (CloseHudObjectProperties) Properties;

		public CloseHudObject(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			UiManager.RemoveHudObject(MyProperties.HudObjectName);
		}
	}
}
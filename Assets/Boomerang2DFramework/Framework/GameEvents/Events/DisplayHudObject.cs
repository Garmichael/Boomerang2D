using Boomerang2DFramework.Framework.UiManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class DisplayHudObject : GameEvent {
		private DisplayHudObjectProperties MyProperties => (DisplayHudObjectProperties) Properties;

		public DisplayHudObject(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			UiManager.DisplayHudObject(MyProperties.HudObject, MyProperties.OptionalContentId);
		}
	}
}
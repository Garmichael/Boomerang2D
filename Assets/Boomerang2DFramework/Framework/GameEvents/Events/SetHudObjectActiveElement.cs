using Boomerang2DFramework.Framework.UiManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class SetHudObjectActiveElement : GameEvent {
		private SetHudObjectActiveElementProperties MyProperties => (SetHudObjectActiveElementProperties) Properties;

		public SetHudObjectActiveElement(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			UiManager.SetElementActiveStatus(MyProperties.HudObjectName, MyProperties.ElementIndex, MyProperties.IsActive);
		}
	}
}
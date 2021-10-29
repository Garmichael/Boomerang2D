using Boomerang2DFramework.Framework.UiManagement.UiElements;

namespace Boomerang2DFramework.Framework.UiManagement.TriggerSystem {
	/// <summary>
	/// JSON definition 
	/// </summary>
	[System.Serializable]
	public class HudObjectTrigger {
		public HudObjectTrigger() { }

		public HudObjectTrigger(HudObjectTriggerProperties elementTriggerProperties) { }

		public virtual bool IsTriggered(HudObjectBehavior hudObjectBehavior) {
			return false;
		}
	}
}
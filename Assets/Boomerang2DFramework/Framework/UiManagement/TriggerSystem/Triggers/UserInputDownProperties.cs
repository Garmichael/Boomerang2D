using System;

namespace Boomerang2DFramework.Framework.UiManagement.TriggerSystem.Triggers {
	[Serializable]
	public class UserInputDownProperties : HudObjectTriggerProperties {
		public InputType InputType;
		public string InputValue;
	}
}
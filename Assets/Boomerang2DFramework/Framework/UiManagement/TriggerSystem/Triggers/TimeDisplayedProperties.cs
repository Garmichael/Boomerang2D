using System;

namespace Boomerang2DFramework.Framework.UiManagement.TriggerSystem.Triggers {
	[Serializable]
	public class TimeDisplayedProperties : HudObjectTriggerProperties {
		public ValueComparison Comparison = ValueComparison.GreaterThan;
		public float LengthDisplayed;
	}
}
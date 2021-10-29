using System;
using Boomerang2DFramework.Framework.UiManagement.UiElements;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.TriggerSystem.Triggers {
	[Serializable]
	public class TimeDisplayed : HudObjectTrigger {
		[SerializeField] private TimeDisplayedProperties Properties;

		public TimeDisplayed(HudObjectTriggerProperties hudObjectTriggerProperties) {
			Properties = (TimeDisplayedProperties) hudObjectTriggerProperties;
		}

		public override bool IsTriggered(HudObjectBehavior hudObjectBehavior) {
			ValueComparison comparison = Properties.Comparison;
			float lengthInState = Properties.LengthDisplayed;

			return BoomerangUtils.CompareValue(comparison, lengthInState, hudObjectBehavior.TimeDisplayed);
		}
	}
}
using System;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.TriggerSystem {
	/// <summary>
	/// JSON definition for HudObjectTriggers
	///  It stores a reference to the specific Trigger class, it's Property class, and a JSON of its properties.
	/// </summary>
	[Serializable]
	public class HudObjectTriggerBuilder {
		public string TriggerClass;
		public string TriggerPropertyClass;
		public string TriggerProperties = "{}";

		public HudObjectTrigger BuildTrigger() {
			Type triggerType = Type.GetType(TriggerClass);
			Type triggerPropertiesType = Type.GetType(TriggerPropertyClass);

			if (triggerType == null) {
				Debug.LogWarning("Trigger Class" + TriggerClass + " not found");
				return null;
			}

			if (triggerPropertiesType == null) {
				Debug.LogWarning("Trigger Properties Type Not Found: " + TriggerPropertyClass);
				return null;
			}

			HudObjectTriggerProperties gameTriggerProperties =
				(HudObjectTriggerProperties) JsonUtility.FromJson(TriggerProperties, triggerPropertiesType);

			if (gameTriggerProperties == null) {
				Debug.LogWarning(
					"Properties JSON for Trigger Properties " + TriggerProperties + " Failed to Parse"
				);
				return null;
			}

			HudObjectTrigger gameTrigger =
				(HudObjectTrigger) Activator.CreateInstance(triggerType, gameTriggerProperties);
			
			return gameTrigger;
		}
	}
}
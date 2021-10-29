using System;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem {
	/// <summary>
	/// The JSON Object containing references to the specific Trigger Class, the Properties Class, and the property
	/// values. These properties are used to construct the actual objects that the Actor uses
	/// </summary>
	[Serializable]
	public class ActorTriggerBuilder {
		public string TriggerClass;
		public string TriggerPropertyClass;
		public string TriggerProperties = "{}";

		public ActorTrigger BuildTrigger() {
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

			ActorTriggerProperties actorTriggerProperties =
				(ActorTriggerProperties) JsonUtility.FromJson(TriggerProperties, triggerPropertiesType);

			if (actorTriggerProperties == null) {
				Debug.LogWarning(
					"Properties JSON for Trigger Properties " + TriggerProperties + " Failed to Parse"
				);
				return null;
			}

			ActorTrigger actorTrigger = (ActorTrigger) Activator.CreateInstance(triggerType, actorTriggerProperties);
			return actorTrigger;
		}
	}
}
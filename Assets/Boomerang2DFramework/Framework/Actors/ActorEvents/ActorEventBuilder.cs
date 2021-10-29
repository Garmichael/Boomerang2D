using System;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents {
	/// <summary>
	/// This is the class included in an Actor's JSON definition for ActorEvents. 
	///  It stores a reference to the specific ActorEvent class, it's Property class, and a JSON of its properties.
	/// </summary>
	[Serializable]
	public class ActorEventBuilder {
		public float StartTime;
		public bool AffectFilteredActors;
		public string ActorEventClass;
		public string ActorEventPropertiesClass;
		public string ActorEventProperties = "{}";

		/// <summary>
		/// Uses the JSON properties to build an ActorEvent class and set its properties.
		/// </summary>
		/// <returns>ActorEvent</returns>
		public ActorEvent BuildActorEvent() {
			Type actorEventType = Type.GetType(ActorEventClass);
			Type actorEventPropertiesType = Type.GetType(ActorEventPropertiesClass);

			if (actorEventType == null) {
				Debug.LogWarning("ActorEvent Class" + ActorEventClass + " not found");
				return null;
			}

			if (actorEventPropertiesType == null) {
				Debug.LogWarning("Game Event Properties Type Not Found: " + ActorEventPropertiesClass);
				return null;
			}

			ActorEventProperties actorEventProperties =
				(ActorEventProperties) JsonUtility.FromJson(ActorEventProperties, actorEventPropertiesType);

			if (actorEventProperties == null) {
				Debug.LogWarning(
					"Properties JSON for ActorEvent Properties " + ActorEventProperties + " Failed to Parse"
				);
				return null;
			}

			ActorEvent actorEventInstance = (ActorEvent) Activator.CreateInstance(actorEventType, actorEventProperties);
			actorEventInstance.StartTime = StartTime;
			actorEventInstance.AffectFilteredActors = AffectFilteredActors;

			return actorEventInstance;
		}
	}
}
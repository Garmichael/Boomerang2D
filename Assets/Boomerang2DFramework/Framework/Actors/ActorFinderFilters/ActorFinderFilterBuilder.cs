using System;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters {
	/// <summary>
	/// This is the class included in an Actor's JSON definition for ActorFinderFilters. 
	///  It stores a reference to the specific ActorEvent class, it's Property class, and a JSON of its properties.
	/// </summary>
	[Serializable]
	public class ActorFinderFilterBuilder {
		public string FilterClass;
		public string FilterPropertiesClass;
		public string FilterProperties = "{}";
		
		/// <summary>
		/// Uses the JSON properties to build an ActorFinderFilter class and set its properties.
		/// </summary>
		/// <returns>ActorEvent</returns>
		public ActorFinderFilter BuildActorFilter() {
			Type filterType = Type.GetType(FilterClass);
			Type filterPropertiesType = Type.GetType(FilterPropertiesClass);

			if (filterType == null) {
				Debug.LogWarning("Filter Class" + FilterClass + " not found");
				return null;
			}

			if (filterPropertiesType == null) {
				Debug.LogWarning("Filter Properties Type Not Found: " + FilterPropertiesClass);
				return null;
			}

			ActorFinderFilterProperties filterProperties =
				(ActorFinderFilterProperties) JsonUtility.FromJson(FilterProperties, filterPropertiesType);

			if (filterProperties == null) {
				Debug.LogWarning(
					"Properties JSON for Filter Properties " + FilterProperties + " Failed to Parse"
				);
				return null;
			}

			ActorFinderFilter actorFinderFilter = (ActorFinderFilter) Activator.CreateInstance(filterType, filterProperties);
			return actorFinderFilter;
		}
	}
}
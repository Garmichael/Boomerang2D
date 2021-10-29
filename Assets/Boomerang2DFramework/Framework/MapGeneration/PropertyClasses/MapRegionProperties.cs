using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration.PropertyClasses {
	/// <summary>
	/// JSON definition for a placed Map Region
	/// </summary>
	[Serializable]
	public class MapRegionProperties {
		public string Name;
		public List<string> EnteringFlag = new List<string>();
		public Vector2Int Position;
		public Vector2Int Dimensions;
		public List<MapRegionActorEvent> RegionActorEvents = new List<MapRegionActorEvent>();
		public bool FiresOnEnter = true;
		public bool FiresOnExit;
		public bool FiresOnStay;
		public float FireOnStayDelay;
	}
}
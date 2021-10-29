using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class SpecificRayOffsetValueProperties : ActorTriggerProperties {
		public Directions Direction = Directions.Up;
		public int BoundingBoxIndex = 0;
		public List<int> RayItems = new List<int> {0};
		public ValueComparison Comparison = ValueComparison.Equal;
		public ActorFloatValueConstructor Distance;
	}
}
using System;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class DistanceFromScreenEdgeProperties : ActorTriggerProperties {
		public Directions Direction;
		public ValueComparison Comparison;
		public ActorFloatValueConstructor Distance;
	}
}
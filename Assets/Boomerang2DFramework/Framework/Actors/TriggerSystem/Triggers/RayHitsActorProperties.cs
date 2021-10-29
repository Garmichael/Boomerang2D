using System;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class RayHitsActorProperties : ActorTriggerProperties {
		public Directions Direction;
		public string Flag;
		public ValueComparison Comparison;
		public ActorFloatValueConstructor Distance;
	}
}
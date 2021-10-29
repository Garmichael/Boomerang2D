using System;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class DistanceToSolidValueProperties : ActorTriggerProperties {
		public Directions Direction = Directions.Up;
		public ValueComparison Comparison = ValueComparison.Equal;
		public ActorFloatValueConstructor Distance;
	}
}
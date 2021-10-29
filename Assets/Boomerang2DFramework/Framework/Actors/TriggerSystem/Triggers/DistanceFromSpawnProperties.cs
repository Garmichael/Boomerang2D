using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class DistanceFromSpawnProperties : ActorTriggerProperties {
		public ValueComparison Comparison;
		public ActorFloatValueConstructor Distance;
	}
}
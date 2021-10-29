using System;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class RandomNumberProperties : ActorTriggerProperties {
		public ActorFloatValueConstructor MinValue;
		public ActorFloatValueConstructor MaxValue;
		public ValueComparison Comparison;
		public ActorFloatValueConstructor TargetValue;
	}
}
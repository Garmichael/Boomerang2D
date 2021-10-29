using System;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class StatFloatValueProperties : ActorTriggerProperties {
		public string StatName;
		public ValueComparison Comparison = ValueComparison.Equal;
		public ActorFloatValueConstructor StatValue;
	}
}
using System;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class TimeInStateProperties : ActorTriggerProperties {
		public ValueComparison Comparison = ValueComparison.GreaterThan;
		public ActorFloatValueConstructor LengthInState = new ActorFloatValueConstructor {StartValue = 10f};
	}
}
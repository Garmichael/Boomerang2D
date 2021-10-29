using System;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class PositionXProperties : ActorTriggerProperties {
		public ValueComparison Comparison;
		public ActorFloatValueConstructor Value;
	}
}
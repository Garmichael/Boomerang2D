using System;
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class GameFlagFloatValueProperties : ActorTriggerProperties {
		public string FlagName;
		public ValueComparison Comparison = ValueComparison.Equal;
		public ActorFloatValueConstructor FlagValue;
	}
}
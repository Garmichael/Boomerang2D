using Boomerang2DFramework.Framework.Actors.ActorFloatValues;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class MathOnStatProperties : ActorEventProperties {
		public string StatName = "";
		public MathOperation Operation;
		public ActorFloatValueConstructor AdjustmentValue;
	}
}
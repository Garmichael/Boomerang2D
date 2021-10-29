using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SetStatFloatProperties : ActorEventProperties {
		public string StatName = "";
		public ActorFloatValueConstructor NewValue;
	}
}
using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SetGameFlagFloatProperties : ActorEventProperties {
		public string GameFlagName;
		public ActorFloatValueConstructor GameFlagValue;
	}
}
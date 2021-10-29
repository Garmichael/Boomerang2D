using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SnapPositionYProperties : ActorEventProperties {
		public ActorFloatValueConstructor SnapValue = new ActorFloatValueConstructor {StartValue = 0.5f};
	}
}
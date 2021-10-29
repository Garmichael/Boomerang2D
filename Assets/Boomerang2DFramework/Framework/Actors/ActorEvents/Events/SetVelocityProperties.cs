using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SetVelocityProperties : ActorEventProperties {
		public bool UpdateHorizontalVelocity;
		public ActorFloatValueConstructor HorizontalVelocity;

		public bool UpdateVerticalVelocity;
		public ActorFloatValueConstructor VerticalVelocity;
	}
}
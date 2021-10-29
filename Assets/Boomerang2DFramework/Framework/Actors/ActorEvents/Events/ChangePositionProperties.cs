using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class ChangePositionProperties : ActorEventProperties {
		public bool UseThisVelocity;
		public bool IsRelativeChange;
		public bool UpdateX = true;
		public ActorFloatValueConstructor OffsetX;
		public bool UpdateY = true;
		public ActorFloatValueConstructor OffsetY;
	}
}
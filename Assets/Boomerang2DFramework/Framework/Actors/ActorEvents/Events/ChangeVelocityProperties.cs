using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class ChangeVelocityProperties : ActorEventProperties {
		public bool IsRelativeChange;
		public ActorFloatValueConstructor VelocityX;
		public ActorFloatValueConstructor VelocityY;
	}
}
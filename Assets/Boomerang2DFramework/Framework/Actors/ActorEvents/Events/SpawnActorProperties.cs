using Boomerang2DFramework.Framework.Actors.ActorFloatValues;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SpawnActorProperties : ActorEventProperties {
		public string Actor = "";
		public bool RelativePosition = true;
		public ActorFloatValueConstructor PositionX;
		public ActorFloatValueConstructor PositionY;
		public ActorFloatValueConstructor PositionZ;
		public ActorInstanceProperties ActorInstanceProperties = new ActorInstanceProperties();
		public bool StartInDefaultState = true;
		public string StateToStartIn = "";
		public bool SetFacingDirection;
		public Directions FacingDirection;
	}
}
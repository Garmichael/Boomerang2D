using System;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class NotFacingDirectionProperties : ActorTriggerProperties {
		public Directions FacingDirection = Directions.Right;
	}
}
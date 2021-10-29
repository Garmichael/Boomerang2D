using System;
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class PreviousFacingDirectionProperties : ActorTriggerProperties {
		public Directions PreviousFacingDirection = Directions.Up;
	}
}
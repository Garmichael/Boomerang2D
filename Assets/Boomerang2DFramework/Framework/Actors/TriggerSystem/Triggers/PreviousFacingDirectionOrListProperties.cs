using System;
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class PreviousFacingDirectionOrListProperties : ActorTriggerProperties {
		public List<Directions> PreviousFacingDirectionOrList = new List<Directions>();
	}
}
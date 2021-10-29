using System;
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class FacingDirectionOrListProperties : ActorTriggerProperties {
		public List<Directions> FacingDirectionOrList = new List<Directions>();
	}
}
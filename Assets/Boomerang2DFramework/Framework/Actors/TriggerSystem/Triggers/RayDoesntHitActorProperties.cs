using System;
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class RayDoesntHitActorProperties : ActorTriggerProperties {
		public Directions Direction;
		public string Flag;
	}
}
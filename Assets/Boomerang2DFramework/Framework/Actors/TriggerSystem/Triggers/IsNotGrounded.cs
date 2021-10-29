using System;
using Boomerang2DFramework.Framework.StateManagement;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class IsNotGrounded : ActorTrigger {
		public IsNotGrounded(ActorTriggerProperties actorTriggerProperties) { }

		public override bool IsTriggered(Actor actor, State state) {
			return !actor.IsGrounded;
		}
	}
}
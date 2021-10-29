using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class PreviousFacingDirection : ActorTrigger {
		[SerializeField] private PreviousFacingDirectionProperties _properties;

		public PreviousFacingDirection(ActorTriggerProperties actorTriggerProperties) {
			_properties = (PreviousFacingDirectionProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return actor.PreviousFacingDirection == _properties.PreviousFacingDirection;
		}
	}
}
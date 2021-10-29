using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class NotFacingDirection : ActorTrigger {
		[SerializeField] private NotFacingDirectionProperties _properties;

		public NotFacingDirection(ActorTriggerProperties actorTriggerProperties) {
			_properties = (NotFacingDirectionProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return actor.FacingDirection != _properties.FacingDirection;
		}
	}
}
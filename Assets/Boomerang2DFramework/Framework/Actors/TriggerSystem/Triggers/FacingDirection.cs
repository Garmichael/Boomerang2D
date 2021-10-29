using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class FacingDirection : ActorTrigger {
		[SerializeField] private FacingDirectionProperties _properties;

		public FacingDirection(ActorTriggerProperties actorTriggerProperties) {
			_properties = (FacingDirectionProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return actor.FacingDirection == _properties.FacingDirection;
		}
	}
}
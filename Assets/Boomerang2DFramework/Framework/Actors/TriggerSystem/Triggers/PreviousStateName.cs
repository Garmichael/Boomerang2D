using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class PreviousStateName : ActorTrigger {
		[SerializeField] private PreviousStateNameProperties _properties;

		public PreviousStateName(ActorTriggerProperties actorTriggerProperties) {
			_properties = (PreviousStateNameProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return actor.StateMachine.GetPreviousState()?.Name == _properties.StateName;
		}
	}
}
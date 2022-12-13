using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class CurrentStateCompleted : ActorTrigger {
		[SerializeField] private CurrentStateCompletedProperties _properties;

		public CurrentStateCompleted(ActorTriggerProperties actorTriggerProperties) {
			_properties = (CurrentStateCompletedProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return state.CompletedAction;
		}
	}
}
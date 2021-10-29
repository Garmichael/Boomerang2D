using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class CurrentState : ActorTrigger {
		[SerializeField] private CurrentStateProperties _properties;

		public CurrentState(ActorTriggerProperties actorTriggerProperties) {
			_properties = (CurrentStateProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return _properties.StateNames.IndexOf(state.Name) > -1;
		}
	}
}
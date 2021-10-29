using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class CurrentStateNot : ActorTrigger {
		[SerializeField] private CurrentStateNotProperties _properties;

		public CurrentStateNot(ActorTriggerProperties actorTriggerProperties) {
			_properties = (CurrentStateNotProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return _properties.StateNames.IndexOf(state.Name) == -1;
		}
	}
}
using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class FacingDirectionOrList : ActorTrigger {
		[SerializeField] private FacingDirectionOrListProperties _properties;

		public FacingDirectionOrList(ActorTriggerProperties actorTriggerProperties) {
			_properties = (FacingDirectionOrListProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return _properties.FacingDirectionOrList.IndexOf(actor.FacingDirection) > -1;
		}
	}
}
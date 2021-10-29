using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class PreviousFacingDirectionOrList : ActorTrigger {
		[SerializeField] private PreviousFacingDirectionOrListProperties _properties;

		public PreviousFacingDirectionOrList(ActorTriggerProperties actorTriggerProperties) {
			_properties = (PreviousFacingDirectionOrListProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return _properties.PreviousFacingDirectionOrList.IndexOf(actor.PreviousFacingDirection) > -1;
		}
	}
}
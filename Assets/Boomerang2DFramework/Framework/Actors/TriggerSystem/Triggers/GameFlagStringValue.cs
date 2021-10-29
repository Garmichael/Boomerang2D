using System;
using Boomerang2DFramework.Framework.GameFlagManagement;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class GameFlagStringValue : ActorTrigger {
		[SerializeField] private GameFlagStringValueProperties _properties;

		public GameFlagStringValue(ActorTriggerProperties actorTriggerProperties) {
			_properties = (GameFlagStringValueProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return GameFlags.GetStringFlag(_properties.FlagName) == _properties.FlagValue;
		}
	}
}
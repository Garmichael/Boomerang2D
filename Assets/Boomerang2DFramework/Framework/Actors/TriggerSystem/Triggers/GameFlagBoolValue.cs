using System;
using Boomerang2DFramework.Framework.GameFlagManagement;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class GameFlagBoolValue : ActorTrigger {
		[SerializeField] private GameFlagBoolValueProperties _properties;

		public GameFlagBoolValue(ActorTriggerProperties actorTriggerProperties) {
			_properties = (GameFlagBoolValueProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			return GameFlags.GetBoolFlag(_properties.FlagName) == _properties.FlagValue;
		}
	}
}
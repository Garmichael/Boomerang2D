using System;
using Boomerang2DFramework.Framework.GameFlagManagement;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class GameFlagFloatValue : ActorTrigger {
		[SerializeField] private GameFlagFloatValueProperties _properties;

		public GameFlagFloatValue(ActorTriggerProperties actorTriggerProperties) {
			_properties = (GameFlagFloatValueProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			float target = _properties.FlagValue.BuildValue(actor);
			float flagValue = GameFlags.GetFloatFlag(_properties.FlagName);
			
			switch (_properties.Comparison) {
				case ValueComparison.Equal:
					return Math.Abs(flagValue - target) < 0.001;
				case ValueComparison.LessThanOrEqual:
					return flagValue <= target;
				case ValueComparison.LessThan:
					return flagValue < target;
				case ValueComparison.GreaterThanOrEqual:
					return flagValue >= target;
				case ValueComparison.GreaterThan:
					return flagValue > target;
				default:
					return false;
			}
		}
	}
}
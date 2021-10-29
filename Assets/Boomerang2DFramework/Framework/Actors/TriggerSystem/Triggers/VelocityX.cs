using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class VelocityX : ActorTrigger {
		[SerializeField] private VelocityXProperties _properties;

		public VelocityX(ActorTriggerProperties actorTriggerProperties) {
			_properties = (VelocityXProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			float target = _properties.Value.BuildValue(actor);
			return
				_properties.Comparison == ValueComparison.LessThan && actor.Velocity.x < target ||
				_properties.Comparison == ValueComparison.LessThanOrEqual && actor.Velocity.x <= target ||
				_properties.Comparison == ValueComparison.Equal && Math.Abs(actor.Velocity.x - target) < 0.1 ||
				_properties.Comparison == ValueComparison.GreaterThanOrEqual && actor.Velocity.x >= target ||
				_properties.Comparison == ValueComparison.GreaterThan && actor.Velocity.x > target;
		}
	}
}
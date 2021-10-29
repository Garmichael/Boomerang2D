using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class VelocityY : ActorTrigger {
		[SerializeField] private VelocityYProperties _properties;

		public VelocityY(ActorTriggerProperties actorTriggerProperties) {
			_properties = (VelocityYProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			float target = _properties.Value.BuildValue(actor);

			return
				_properties.Comparison == ValueComparison.LessThan && actor.Velocity.y < target ||
				_properties.Comparison == ValueComparison.LessThanOrEqual && actor.Velocity.y <= target ||
				_properties.Comparison == ValueComparison.Equal && Math.Abs(actor.Velocity.y - target) < 0.1 ||
				_properties.Comparison == ValueComparison.GreaterThanOrEqual && actor.Velocity.y >= target ||
				_properties.Comparison == ValueComparison.GreaterThan && actor.Velocity.y > target;
		}
	}
}
using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class PositionX : ActorTrigger {
		[SerializeField] private PositionXProperties _properties;

		public PositionX(ActorTriggerProperties actorTriggerProperties) {
			_properties = (PositionXProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			float target = _properties.Value.BuildValue(actor);
			return
				_properties.Comparison == ValueComparison.LessThan && actor.Transform.localPosition.x < target ||
				_properties.Comparison == ValueComparison.LessThanOrEqual && actor.Transform.localPosition.x <= target ||
				_properties.Comparison == ValueComparison.Equal && Math.Abs(actor.Transform.localPosition.x - target) < 0.1 ||
				_properties.Comparison == ValueComparison.GreaterThanOrEqual && actor.Transform.localPosition.x >= target ||
				_properties.Comparison == ValueComparison.GreaterThan && actor.Transform.localPosition.x > target;
		}
	}
}
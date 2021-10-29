using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class PositionY : ActorTrigger {
		[SerializeField] private PositionYProperties _properties;

		public PositionY(ActorTriggerProperties actorTriggerProperties) {
			_properties = (PositionYProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			float target = _properties.Value.BuildValue(actor);

			return
				_properties.Comparison == ValueComparison.LessThan && actor.Transform.localPosition.y < target ||
				_properties.Comparison == ValueComparison.LessThanOrEqual && actor.Transform.localPosition.y <= target ||
				_properties.Comparison == ValueComparison.Equal && Math.Abs(actor.Transform.localPosition.y - target) < 0.1 ||
				_properties.Comparison == ValueComparison.GreaterThanOrEqual && actor.Transform.localPosition.y >= target ||
				_properties.Comparison == ValueComparison.GreaterThan && actor.Transform.localPosition.y > target;
		}
	}
}
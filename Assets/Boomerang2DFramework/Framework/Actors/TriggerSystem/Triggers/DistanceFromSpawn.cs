using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class DistanceFromSpawn : ActorTrigger {
		[SerializeField] private DistanceFromSpawnProperties _properties;

		public DistanceFromSpawn(ActorTriggerProperties actorTriggerProperties) {
			_properties = (DistanceFromSpawnProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			float distance = Vector2.Distance(actor.SpawnPosition, actor.Transform.localPosition);
			distance = Math.Abs(distance);
			float target = _properties.Distance.BuildValue(actor);

			return _properties.Comparison == ValueComparison.Equal && Math.Abs(distance - target) < 0.001 ||
			       _properties.Comparison == ValueComparison.GreaterThan && distance > target ||
			       _properties.Comparison == ValueComparison.GreaterThanOrEqual && distance >= target ||
			       _properties.Comparison == ValueComparison.LessThan && distance < target ||
			       _properties.Comparison == ValueComparison.LessThanOrEqual && distance <= target;
		}
	}
}
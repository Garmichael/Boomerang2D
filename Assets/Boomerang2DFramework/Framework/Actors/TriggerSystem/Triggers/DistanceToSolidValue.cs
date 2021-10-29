using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class DistanceToSolidValue : ActorTrigger {
		[SerializeField] private DistanceToSolidValueProperties _properties;

		public DistanceToSolidValue(ActorTriggerProperties actorTriggerProperties) {
			_properties = (DistanceToSolidValueProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			Directions direction = _properties.Direction;
			float distance = _properties.Distance.BuildValue(actor);
			ValueComparison comparison = _properties.Comparison;

			float distanceToCheck;

			switch (direction) {
				case Directions.Up:
					distanceToCheck = actor.DistanceToSolidUp;
					break;
				case Directions.Down:
					distanceToCheck = actor.DistanceToSolidDown;
					break;
				case Directions.Left:
					distanceToCheck = actor.DistanceToSolidLeft;
					break;
				case Directions.Right:
					distanceToCheck = actor.DistanceToSolidRight;
					break;
				default:
					distanceToCheck = 0;
					break;
			}

			switch (comparison) {
				case ValueComparison.Equal:
					return Math.Abs(distanceToCheck - distance) < 0.001;
				case ValueComparison.LessThanOrEqual:
					return distanceToCheck <= distance;
				case ValueComparison.LessThan:
					return distanceToCheck < distance;
				case ValueComparison.GreaterThanOrEqual:
					return distanceToCheck >= distance;
				case ValueComparison.GreaterThan:
					return distanceToCheck > distance;
				default:
					return false;
			}
		}
	}
}
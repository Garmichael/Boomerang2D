using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class RayHitsActor : ActorTrigger {
		[SerializeField] private RayHitsActorProperties _properties;

		public RayHitsActor(ActorTriggerProperties actorTriggerProperties) {
			_properties = (RayHitsActorProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			bool meetsCriteria = false;
			float target = _properties.Distance.BuildValue(actor);

			List<Actor.ActorBoundingBoxes> rayHitsCollectionToUse;

			switch (_properties.Direction) {
				case Directions.Up:
					rayHitsCollectionToUse = actor.ActorBoundingBoxesHitByRaysUp;
					break;
				case Directions.Right:
					rayHitsCollectionToUse = actor.ActorBoundingBoxesHitByRaysRight;
					break;
				case Directions.Down:
					rayHitsCollectionToUse = actor.ActorBoundingBoxesHitByRaysDown;
					break;
				default:
					rayHitsCollectionToUse = actor.ActorBoundingBoxesHitByRaysLeft;
					break;
			}

			foreach (Actor.ActorBoundingBoxes boundingBox in rayHitsCollectionToUse) {
				if (boundingBox.BoundingBoxProperties.Flags.IndexOf(_properties.Flag) > -1) {
					if (
						_properties.Comparison == ValueComparison.Equal && Math.Abs(boundingBox.HitDistance - target) < 0.001 ||
						_properties.Comparison == ValueComparison.GreaterThan && boundingBox.HitDistance > target ||
						_properties.Comparison == ValueComparison.GreaterThanOrEqual && boundingBox.HitDistance >= target ||
						_properties.Comparison == ValueComparison.LessThan && boundingBox.HitDistance < target ||
						_properties.Comparison == ValueComparison.LessThanOrEqual && boundingBox.HitDistance <= target
					) {
						meetsCriteria = true;
					}
				}
			}

			return meetsCriteria;
		}
	}
}
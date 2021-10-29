using System;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class SpecificRayOffsetValue : ActorTrigger {
		[SerializeField] private SpecificRayOffsetValueProperties _properties;

		public SpecificRayOffsetValue(ActorTriggerProperties actorTriggerProperties) {
			_properties = (SpecificRayOffsetValueProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			bool allMatched = true;
			float target = _properties.Distance.BuildValue(actor);

			foreach (int rayItem in _properties.RayItems) {
				float offsetDistance = 2000f;
				
				switch (_properties.Direction) {
					case Directions.Up:
						if (
							BoomerangUtils.IndexInRange(actor.RayDataUp, _properties.BoundingBoxIndex) &&
							BoomerangUtils.IndexInRange(actor.RayDataUp[_properties.BoundingBoxIndex], rayItem) &&
							actor.RayDataUp[_properties.BoundingBoxIndex][rayItem].Count > 0
						) {
							offsetDistance = actor.RayDataUp[_properties.BoundingBoxIndex][rayItem][0].distance;
						}

						break;
					case Directions.Right:
						if (
							BoomerangUtils.IndexInRange(actor.RayDataRight, _properties.BoundingBoxIndex) &&
							BoomerangUtils.IndexInRange(actor.RayDataRight[_properties.BoundingBoxIndex], rayItem) &&
							actor.RayDataRight[_properties.BoundingBoxIndex][rayItem].Count > 0
						) {
							offsetDistance = actor.RayDataRight[_properties.BoundingBoxIndex][rayItem][0].distance;
						}

						break;
					case Directions.Down:
						if (
							BoomerangUtils.IndexInRange(actor.RayDataDown, _properties.BoundingBoxIndex) &&
							BoomerangUtils.IndexInRange(actor.RayDataDown[_properties.BoundingBoxIndex], rayItem) &&
							actor.RayDataDown[_properties.BoundingBoxIndex][rayItem].Count > 0
						) {
							offsetDistance = actor.RayDataDown[_properties.BoundingBoxIndex][rayItem][0].distance;
						}

						break;
					default:
						if (
							BoomerangUtils.IndexInRange(actor.RayDataLeft, _properties.BoundingBoxIndex) &&
							BoomerangUtils.IndexInRange(actor.RayDataLeft[_properties.BoundingBoxIndex], rayItem) &&
							actor.RayDataLeft[_properties.BoundingBoxIndex][rayItem].Count > 0
						) {
							offsetDistance = actor.RayDataLeft[_properties.BoundingBoxIndex][rayItem][0].distance;
						}

						break;
				}

				bool distanceRequirementMade =
					_properties.Comparison == ValueComparison.Equal && Math.Abs(offsetDistance - target) < 0.001 ||
					_properties.Comparison == ValueComparison.GreaterThan && offsetDistance > target ||
					_properties.Comparison == ValueComparison.GreaterThanOrEqual && offsetDistance >= target ||
					_properties.Comparison == ValueComparison.LessThan && offsetDistance < target ||
					_properties.Comparison == ValueComparison.LessThanOrEqual && offsetDistance <= target;

				if (!distanceRequirementMade) {
					allMatched = false;
					break;
				}
			}

			return allMatched;
		}
	}
}
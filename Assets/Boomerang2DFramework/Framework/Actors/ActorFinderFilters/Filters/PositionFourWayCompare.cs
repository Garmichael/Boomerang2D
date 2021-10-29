using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class PositionFourWayCompare : ActorFinderFilter {
		private PositionFourWayCompareProperties MyProperties => (PositionFourWayCompareProperties) Properties;

		public PositionFourWayCompare(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			Vector2 origin = originActor.Transform.localPosition;

			foreach (Actor actor in actorCatalog) {
				Directions directionOfActor;

				Vector2 other = new Vector2(actor.Transform.localPosition.x, actor.Transform.localPosition.y);

				Vector2 difference = (other - origin).normalized;
				float angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

				if (angle < 0) {
					angle += 360;
				}

				if (Math.Abs(angle - 360) < 0.1) {
					angle = 0;
				}

				if (angle >= 315 || angle < 45) {
					directionOfActor = Directions.Right;
				} else if (angle >= 45 && angle < 135) {
					directionOfActor = Directions.Up;
				} else if (angle >= 135 && angle < 225) {
					directionOfActor = Directions.Left;
				} else {
					directionOfActor = Directions.Down;
				}

				if (directionOfActor == MyProperties.PositionIs) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Utilities;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class HitByRay : ActorFinderFilter {
		private HitByRayProperties MyProperties => (HitByRayProperties) Properties;

		public HitByRay(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();
			List<Actor.ActorBoundingBoxes> actorsHitByRay;
			float distance = MyProperties.Distance.BuildValue(originActor);

			switch (MyProperties.Direction) {
				case Directions.Up:
					actorsHitByRay = originActor.ActorBoundingBoxesHitByRaysUp;
					break;
				case Directions.Right:
					actorsHitByRay = originActor.ActorBoundingBoxesHitByRaysRight;
					break;
				case Directions.Down:
					actorsHitByRay = originActor.ActorBoundingBoxesHitByRaysDown;
					break;
				default:
					actorsHitByRay = originActor.ActorBoundingBoxesHitByRaysLeft;
					break;
			}

			foreach (Actor.ActorBoundingBoxes actorHitByRay in actorsHitByRay) {
				bool meetsCriteria = BoomerangUtils.CompareValue(MyProperties.Comparison, distance, actorHitByRay.HitDistance);

				if (meetsCriteria) {
					matchingActors.Add(actorHitByRay.Actor);
				}
			}

			return matchingActors;
		}
	}
}
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class DistanceFromOrigin : ActorFinderFilter {
		private DistanceFromOriginProperties MyProperties => (DistanceFromOriginProperties) Properties;

		public DistanceFromOrigin(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();
			float value = MyProperties.Value.BuildValue(originActor);

			foreach (Actor actor in actorCatalog) {
				float distance = Vector2.Distance(
					new Vector2(actor.Transform.localPosition.x, actor.Transform.localPosition.y),
					new Vector2(originActor.Transform.localPosition.x, originActor.Transform.localPosition.y)
				);

				bool meetCriteria = BoomerangUtils.CompareValue(MyProperties.Comparison, value, distance);

				if (meetCriteria) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
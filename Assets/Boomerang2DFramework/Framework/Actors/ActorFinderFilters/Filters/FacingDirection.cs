using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class FacingDirection : ActorFinderFilter {
		private FacingDirectionProperties MyProperties => (FacingDirectionProperties) Properties;

		public FacingDirection(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			foreach (Actor actor in actorCatalog) {
				if (actor.FacingDirection == MyProperties.FacingDirection) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class IsGrounded : ActorFinderFilter {
		public IsGrounded(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			foreach (Actor actor in actorCatalog) {
				if (actor.IsGrounded) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class IsChild : ActorFinderFilter {
		public IsChild(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			foreach (Actor childActor in originActor.ChildrenActors) {
				if (actorCatalog.Contains(childActor)) {
					matchingActors.Add(childActor);
				}
			}

			return matchingActors;
		}
	}
}
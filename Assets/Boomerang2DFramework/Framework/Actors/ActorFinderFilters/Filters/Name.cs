using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class Name : ActorFinderFilter {
		private NameProperties MyProperties => (NameProperties) Properties;

		public Name(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			foreach (Actor actor in actorCatalog) {
				if (actor.ActorProperties.Name == MyProperties.Name) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
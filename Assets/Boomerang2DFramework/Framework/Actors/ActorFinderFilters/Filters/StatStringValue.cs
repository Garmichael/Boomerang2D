using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class StatStringValue : ActorFinderFilter {
		private StatStringValueProperties MyProperties => (StatStringValueProperties) Properties;

		public StatStringValue(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			foreach (Actor actor in actorCatalog) {
				foreach (StringStatProperties stat in actor.ActorProperties.StatsStrings) {
					if (stat.Name == MyProperties.StatName && stat.Value == MyProperties.Value) {
						matchingActors.Add(actor);
					}
				}
			}

			return matchingActors;
		}
	}
}
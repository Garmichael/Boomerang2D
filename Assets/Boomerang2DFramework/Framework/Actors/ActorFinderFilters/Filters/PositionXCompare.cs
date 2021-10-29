using System.Collections.Generic;
using Boomerang2DFramework.Framework.Utilities;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class PositionXCompare : ActorFinderFilter {
		private PositionXCompareProperties MyProperties => (PositionXCompareProperties) Properties;

		public PositionXCompare(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			foreach (Actor actor in actorCatalog) {
				bool meetsCriteria = BoomerangUtils.CompareValue(
					MyProperties.Comparison,
					originActor.Transform.localPosition.x,
					actor.Transform.localPosition.x
				);

				if (meetsCriteria) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
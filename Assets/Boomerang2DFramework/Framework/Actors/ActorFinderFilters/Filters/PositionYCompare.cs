using System.Collections.Generic;
using Boomerang2DFramework.Framework.Utilities;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class PositionYCompare : ActorFinderFilter {
		private PositionYCompareProperties MyProperties => (PositionYCompareProperties) Properties;

		public PositionYCompare(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			foreach (Actor actor in actorCatalog) {
				bool meetsCriteria = BoomerangUtils.CompareValue(
					MyProperties.Comparison,
					originActor.Transform.localPosition.y,
					actor.Transform.localPosition.y
				);

				if (meetsCriteria) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
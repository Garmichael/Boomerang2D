using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class StatFloatValue : ActorFinderFilter {
		private StatFloatValueProperties MyProperties => (StatFloatValueProperties) Properties;

		public StatFloatValue(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();
			float value = MyProperties.Value.BuildValue(originActor);

			foreach (Actor actor in actorCatalog) {
				foreach (FloatStatProperties stat in actor.ActorProperties.StatsFloats) {
					if (stat.Name == MyProperties.StatName) {
						bool meetsCriteria = BoomerangUtils.CompareValue(MyProperties.Comparison, value, stat.Value);

						if (meetsCriteria) {
							matchingActors.Add(actor);
						}
					}
				}
			}

			return matchingActors;
		}
	}
}
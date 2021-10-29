using System.Collections.Generic;
using Boomerang2DFramework.Framework.Utilities;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class PositionY : ActorFinderFilter {
		private PositionYProperties MyProperties => (PositionYProperties) Properties;

		public PositionY(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();
			float value = MyProperties.Value.BuildValue(originActor);

			foreach (Actor actor in actorCatalog) {
				bool meetsCriteria = BoomerangUtils.CompareValue(MyProperties.Comparison, value, actor.Transform.localPosition.y);

				if (meetsCriteria) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Utilities;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class VelocityY : ActorFinderFilter {
		private VelocityYProperties MyProperties => (VelocityYProperties) Properties;

		public VelocityY(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();
			float value = MyProperties.Value.BuildValue(originActor);

			foreach (Actor actor in actorCatalog) {
				bool meetsCriteria = BoomerangUtils.CompareValue(MyProperties.Comparison, value, actor.Velocity.y);

				if (meetsCriteria) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
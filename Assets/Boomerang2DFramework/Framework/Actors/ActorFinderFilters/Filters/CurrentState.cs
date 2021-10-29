using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class CurrentState : ActorFinderFilter {
		private CurrentStateProperties MyProperties => (CurrentStateProperties) Properties;

		public CurrentState(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			foreach (Actor actor in actorCatalog) {
				if (MyProperties.StateNames.Contains(actor.StateMachine.GetCurrentState().Name)) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}
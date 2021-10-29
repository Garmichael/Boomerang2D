using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class OverlapsActorCollider : ActorFinderFilter {
		private OverlapsActorColliderProperties MyProperties => (OverlapsActorColliderProperties) Properties;

		public OverlapsActorCollider(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			if (!MyProperties.UseAnySelfFlag) {
				if (originActor.OverlappingActorFlags.ContainsKey(MyProperties.SelfFlag)) {
					foreach (Actor.OverlappingCollider overlappingCollider in originActor.OverlappingActorFlags[MyProperties.SelfFlag]) {
						if (MyProperties.UseAnyOtherFlag || overlappingCollider.Flag == MyProperties.OtherFlag) {
							matchingActors.Add(overlappingCollider.Actor);
						}
					}
				}
			} else {
				foreach (KeyValuePair<string, List<Actor.OverlappingCollider>> selfFlag in originActor.OverlappingActorFlags) {
					foreach (Actor.OverlappingCollider overlappingCollider in selfFlag.Value) {
						if (MyProperties.UseAnyOtherFlag || overlappingCollider.Flag == MyProperties.OtherFlag) {
							matchingActors.Add(overlappingCollider.Actor);
						}
					}
				}
			}

			return matchingActors;
		}
	}
}
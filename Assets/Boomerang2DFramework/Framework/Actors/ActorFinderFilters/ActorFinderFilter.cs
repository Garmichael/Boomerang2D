using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters {
	/// <summary>
	/// Actor FinderFilters search the Map's Actor Catalog for Actors that match the filter criteria 
	/// </summary>
	public class ActorFinderFilter {
		protected ActorFinderFilterProperties Properties;

		/// <summary>
		/// Searches the given ActorCatalog to find Actors matching the filter
		/// </summary>
		/// <param name="originActor">The Actor that the filter is being executed on</param>
		/// <param name="actorCatalog">The collection of actors to check</param>
		/// <returns></returns>
		public virtual List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			return new List<Actor>();
		}
	}
}
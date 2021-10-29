using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	[System.Serializable]
	public class HitByRayProperties : ActorFinderFilterProperties {
		public Directions Direction;
		public ValueComparison Comparison;
		public ActorFloatValueConstructor Distance;
	}
}
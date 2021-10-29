using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	[System.Serializable]
	public class StatFloatValueProperties : ActorFinderFilterProperties {
		public string StatName;
		public ValueComparison Comparison;
		public ActorFloatValueConstructor Value;
	}
}
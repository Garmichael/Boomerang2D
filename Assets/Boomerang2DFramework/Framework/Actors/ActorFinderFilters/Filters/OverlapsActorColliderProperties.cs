namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	[System.Serializable]
	public class OverlapsActorColliderProperties : ActorFinderFilterProperties {
		public bool UseAnySelfFlag = false;
		public string SelfFlag = "";
		public bool UseAnyOtherFlag = false;
		public string OtherFlag = "";
	}
}
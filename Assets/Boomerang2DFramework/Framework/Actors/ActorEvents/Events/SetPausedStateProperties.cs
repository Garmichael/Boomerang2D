namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SetPausedStateProperties : ActorEventProperties {
		public bool AffectAllActors;
		public bool IsPaused;
	}
}
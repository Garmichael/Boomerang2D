namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SetStatBoolProperties : ActorEventProperties {
		public string StatName = "";
		public bool NewValue;
	}
}
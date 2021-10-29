namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class DisplayHudObjectProperties : ActorEventProperties {
		public string HudObject;
		public string OptionalContentId;
		public bool UsesStatForContentId;
		public string StatName;
	}
}
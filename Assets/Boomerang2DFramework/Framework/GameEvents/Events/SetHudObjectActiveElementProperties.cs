namespace Boomerang2DFramework.Framework.GameEvents.Events {
	[System.Serializable]
	public class SetHudObjectActiveElementProperties : GameEventProperties {
		public string HudObjectName;
		public int ElementIndex;
public bool IsActive;
	}
}
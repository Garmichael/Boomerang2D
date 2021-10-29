using System;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class HudObjectDisplayedProperties : ActorTriggerProperties {
		public string HudObjectName;
		public bool IsOpened = true;
	}
}
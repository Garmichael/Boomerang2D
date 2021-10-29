using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	[System.Serializable]
	public class DisplayHudObjectChainProperties : GameEventProperties {
		public List<string> Chain;
		public List<string> ContentIds;
	}
}
using System.Collections.Generic;
using Boomerang2DFramework.Framework.UiManagement.InteractionEvents;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements {
	/// <summary>
	/// JSON definition for a HudObject
	/// </summary>
	[System.Serializable]
	public class HudObjectProperties {
		public string Name;
		public string Shader = "Unlit/Transparent";
		public List<HudElementProperties> Elements = new List<HudElementProperties>();
		public List<HudObjectInteractionEvent> InteractionEvents = new List<HudObjectInteractionEvent>();
	}
}
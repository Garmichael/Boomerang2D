using System.Collections.Generic;
using Boomerang2DFramework.Framework.GameEvents;
using Boomerang2DFramework.Framework.UiManagement.TriggerSystem;

namespace Boomerang2DFramework.Framework.UiManagement.InteractionEvents {
	/// <summary>
	/// JSON definition for a HudObject Interaction Event
	/// </summary>
	[System.Serializable]
	public class HudObjectInteractionEvent {
		public string Name;
		public bool FireOnce;
		public bool HasExecuted;
		public bool Enabled = true;
		
		public List<HudObjectTriggerBuilder> TriggerBuilders = new List<HudObjectTriggerBuilder>();
		public List<GameEventBuilder> GameEventBuilders = new List<GameEventBuilder>();

		public List<HudObjectTrigger> Triggers = new List<HudObjectTrigger>();
		public List<GameEvent> GameEvents = new List<GameEvent>();
	}
}
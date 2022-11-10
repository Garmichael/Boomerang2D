using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.Actors.PropertyClasses {
	/// <summary>
	/// JSON definition for the String Stats properties of an Actor 
	/// </summary>
	[System.Serializable]
	public class StateEntryExitEventProperties {
		public List<ActorTriggerBuilder> ActorTriggerBuilders = new();
		public List<ActorEventBuilder> ActorEventBuilders = new();
		public List<ActorEventBuilder> ActorElseEventBuilders = new();

		public List<ActorTrigger> Triggers = new();
		public List<ActorEvent> ActorEvents = new();
		public List<ActorEvent> ActorElseEvents = new();
	}
}
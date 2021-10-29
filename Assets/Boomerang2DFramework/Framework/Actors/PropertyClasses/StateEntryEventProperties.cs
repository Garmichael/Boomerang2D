using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.Actors.PropertyClasses {
	/// <summary>
	/// JSON definition for the String Stats properties of an Actor 
	/// </summary>
	[System.Serializable]
	public class StateEntryEventProperties {
		public List<ActorTriggerBuilder> ActorTriggerBuilders = new List<ActorTriggerBuilder>();
		public List<ActorEventBuilder> ActorEventBuilders = new List<ActorEventBuilder>();

		public List<ActorTrigger> Triggers = new List<ActorTrigger>();
		public List<ActorEvent> ActorEvents = new List<ActorEvent>();
	}
}
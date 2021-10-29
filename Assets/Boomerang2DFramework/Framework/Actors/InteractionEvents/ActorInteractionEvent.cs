using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.ActorFinderFilters;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.Actors.InteractionEvents {
	/// <summary>
	/// JSON for defining the Actor Interaction Events
	/// </summary>
	[System.Serializable]
	public class ActorInteractionEvent {
		public string Name;
		public bool HasExecuted;
		public bool Enabled = true;

		public List<ActorFinderFilterBuilder> ActorFinderFilterBuilders = new List<ActorFinderFilterBuilder>();
		public ValueComparison FoundActorsComparison;
		public int FoundActorsCount;

		public List<ActorTriggerBuilder> ActorTriggerBuilders = new List<ActorTriggerBuilder>();
		public List<ActorEventBuilder> ActorEventBuilders = new List<ActorEventBuilder>();

		public List<ActorTrigger> Triggers = new List<ActorTrigger>();
		public List<ActorFinderFilter> AfterFinderFilters = new List<ActorFinderFilter>();
		public List<ActorEvent> ActorEvents = new List<ActorEvent>();
	}
}
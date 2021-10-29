using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.MapGeneration.PropertyClasses {
	/// <summary>
	/// JSON definition for the Events that fire when an Actor interacts with a Map Region
	/// </summary>
	[Serializable]
	public class MapRegionActorEvent {
		public List<ActorTriggerBuilder> ActorTriggerBuilders = new List<ActorTriggerBuilder>();
		public List<ActorEventBuilder> ActorEventBuilders = new List<ActorEventBuilder>();

		public List<ActorTrigger> Triggers = new List<ActorTrigger>();
		public List<ActorEvent> ActorEvents = new List<ActorEvent>();
	}
}
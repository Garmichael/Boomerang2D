using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties.TriggerProperties {
	/// <summary>
	/// JSON definition for Transition Triggers
	/// </summary>
	[System.Serializable]
	public class TransitionTriggerProperties {
		public string NextState;
		public List<ActorTriggerBuilder> ActorTriggerBuilders = new List<ActorTriggerBuilder>();
	}
}
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties.TriggerProperties {
	/// <summary>
	/// JSON definition for Modification Trigger Properties
	/// </summary>
	[System.Serializable]
	public class RandomTransitionTriggerProperties {
		public List<ActorTriggerBuilder> ActorTriggerBuilders = new List<ActorTriggerBuilder>();
		public List<RandomTransitionOption> TransitionOptions = new List<RandomTransitionOption>();
	}
}
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties.AnimationProperties {
	/// <summary>
	/// JSON definition for a State Animation's Condition set
	/// </summary>
	[System.Serializable]
	public class StatePropertiesAnimationCondition {
		public List<ActorTriggerBuilder> ActorTriggerBuilders = new List<ActorTriggerBuilder>();
	}
}
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties.TriggerProperties {
	/// <summary>
	/// JSON definition for Modification Triggers
	/// </summary>
	[System.Serializable]
	public class ModificationTriggerProperties {
		public string Method;
		public List<ActorTriggerBuilder> ActorTriggerBuilders = new List<ActorTriggerBuilder>();
	}
}
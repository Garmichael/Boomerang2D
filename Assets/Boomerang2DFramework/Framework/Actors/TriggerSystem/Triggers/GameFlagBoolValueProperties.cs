using System;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class GameFlagBoolValueProperties : ActorTriggerProperties {
		public string FlagName;
		public bool FlagValue;
	}
}
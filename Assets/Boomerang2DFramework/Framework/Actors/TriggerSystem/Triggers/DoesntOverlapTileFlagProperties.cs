using System;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class DoesntOverlapTileFlagProperties : ActorTriggerProperties {
		public bool UseAnySelfFlag = false;
		public string SelfFlag = "";
		public string OtherFlag = "";
	}
}
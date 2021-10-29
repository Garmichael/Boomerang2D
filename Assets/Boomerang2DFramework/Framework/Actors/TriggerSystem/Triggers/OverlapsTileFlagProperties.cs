using System;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class OverlapsTileFlagProperties : ActorTriggerProperties {
		public bool UseAnySelfFlag;
		public string SelfFlag = "";
		public string OtherFlag = "";
	}
}
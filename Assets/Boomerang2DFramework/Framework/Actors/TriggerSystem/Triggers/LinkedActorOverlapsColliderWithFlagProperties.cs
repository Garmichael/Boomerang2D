using System;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class LinkedActorOverlapsColliderWithFlagProperties : ActorTriggerProperties {
		public int LinkedActorSlotId;
		public bool UseAnySelfFlag;
		public string SelfFlag = "";
		public string OtherFlag = "";
	}
}